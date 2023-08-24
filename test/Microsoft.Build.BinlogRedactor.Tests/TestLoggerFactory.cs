// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    public sealed class TestLoggerFactory : ILoggerFactory
    {
        private readonly List<ILoggerProvider> _loggerProviders = new List<ILoggerProvider>();

        private readonly List<ILoggerFactory> _factories = new List<ILoggerFactory>();

        public TestLoggerFactory(IMessageSink? messageSink = null)
        {
            if (messageSink != null)
            {
                SharedTestOutputHelper testOutputHelper = new SharedTestOutputHelper(messageSink);
                _loggerProviders =
                    new List<ILoggerProvider>() { new XunitLoggerProvider(testOutputHelper) };
            }
        }

        public void Dispose()
        {
            while (_factories.Count > 0)
            {
                var factory = _factories[0];
                _factories.RemoveAt(0);

                factory?.Dispose();
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace);

                if (_loggerProviders?.Any() ?? false)
                {
                    foreach (ILoggerProvider loggerProvider in _loggerProviders)
                    {
                        builder.AddProvider(loggerProvider);
                    }
                }
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                    options.IncludeScopes = true;
                });
            });
            _factories.Add(loggerFactory);
            return loggerFactory.CreateLogger(categoryName);
        }

        public ILogger CreateLogger() => CreateLogger("Test Host");

        public void AddProvider(ILoggerProvider provider)
        {
            _loggerProviders.Add(provider);
        }
    }

    public sealed class SharedTestOutputHelper : ITestOutputHelper
    {
        private readonly IMessageSink _sink;

        public SharedTestOutputHelper(IMessageSink sink)
        {
            this._sink = sink;
        }

        public void WriteLine(string message)
        {
            _sink.OnMessage(new DiagnosticMessage(message));
        }

        public void WriteLine(string format, params object[] args)
        {
            _sink.OnMessage(new DiagnosticMessage(format, args));
        }
    }

    /// <summary>
    /// Microsoft.Extensions.Logging <see cref="ILoggerProvider"/> which logs to XUnit test output.
    /// </summary>
    /// <remarks>
    /// See https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging/tests/DI.Common/Common/src/XunitLoggerProvider.cs for more details.
    /// </remarks>
    public sealed class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;
        private readonly LogLevel _minLevel;
        private readonly DateTimeOffset? _logStart;

        public XunitLoggerProvider(ITestOutputHelper output)
            : this(output, LogLevel.Trace)
        {
        }

        public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
            : this(output, minLevel, null)
        {
        }

        public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel, DateTimeOffset? logStart)
        {
            _output = output;
            _minLevel = minLevel;
            _logStart = logStart;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, categoryName, _minLevel, _logStart);
        }

        public void Dispose()
        {
        }

        private sealed class XunitLogger : ILogger
        {
            private static readonly string[] NewLineChars = new[] { Environment.NewLine };
            private readonly string _category;
            private readonly LogLevel _minLogLevel;
            private readonly ITestOutputHelper _output;
            private readonly DateTimeOffset? _logStart;

            public XunitLogger(ITestOutputHelper output, string category, LogLevel minLogLevel, DateTimeOffset? logStart)
            {
                _minLogLevel = minLogLevel;
                _category = category;
                _output = output;
                _logStart = logStart;
            }

            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                // Buffer the message into a single string in order to avoid shearing the message when running across multiple threads.
                var messageBuilder = new StringBuilder();

                var timestamp = _logStart.HasValue ? $"{(DateTimeOffset.UtcNow - _logStart.Value).TotalSeconds:N3}s" : DateTimeOffset.UtcNow.ToString("s");

                var firstLinePrefix = $"| [{timestamp}] {_category} {logLevel}: ";
                var lines = formatter(state, exception).Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                messageBuilder.AppendLine(firstLinePrefix + lines.FirstOrDefault() ?? string.Empty);

                var additionalLinePrefix = "|" + new string(' ', firstLinePrefix.Length - 1);
                foreach (var line in lines.Skip(1))
                {
                    messageBuilder.AppendLine(additionalLinePrefix + line);
                }

                if (exception != null)
                {
                    lines = exception.ToString().Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    additionalLinePrefix = "| ";
                    foreach (var line in lines)
                    {
                        messageBuilder.AppendLine(additionalLinePrefix + line);
                    }
                }

                // Remove the last line-break, because ITestOutputHelper only has WriteLine.
                var message = messageBuilder.ToString();
                if (message.EndsWith(Environment.NewLine))
                {
                    message = message.Substring(0, message.Length - Environment.NewLine.Length);
                }

                try
                {
                    _output.WriteLine(message);
                }
                catch (Exception)
                {
                    // We could fail because we're on a background thread and our captured ITestOutputHelper is
                    // busted (if the test "completed" before the background thread fired).
                    // So, ignore this. There isn't really anything we can do but hope the
                    // caller has additional loggers registered
                }
            }

            public bool IsEnabled(LogLevel logLevel)
                => logLevel >= _minLogLevel;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
                => new NullScope();

            private sealed class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
