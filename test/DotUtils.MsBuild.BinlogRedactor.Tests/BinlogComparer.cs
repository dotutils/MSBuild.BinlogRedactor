// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.Logging;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    internal static class BinlogComparer
    {
        internal static void AssertFilesAreBinaryEqual(string firstPath, string secondPath)
        {
            FileInfo first = new(firstPath);
            FileInfo second = new(secondPath);

            // Skipping shortcut test - so that we can better troubleshoot failures.
            ////if (first.Length != second.Length)
            ////{
            ////    Assert.Fail($"Files differ in size ({first.Name}:{first.Length} and {second.Name}:{second.Length}");
            ////}

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using FileStream fs1 = first.OpenRead();
            using FileStream fs2 = second.OpenRead();
            for (int i = 0; i < Math.Min(first.Length, second.Length); i++)
            {
                byte b1 = (byte)fs1.ReadByte();
                byte b2 = (byte)fs2.ReadByte();
                if (b1 != b2)
                {
                    Assert.Fail(
                        $"Files ({first.Name}:{first.Length} and {second.Name}:{second.Length} sizes) are not equal at byte {i} ({b1} vs {b2})");
                }
            }

            if (first.Length != second.Length)
            {
                Assert.Fail($"Files differ in size ({first.Name}:{first.Length} and {second.Name}:{second.Length}");
            }
        }

        internal static void AssertBinlogsHaveEqualContent(string firstPath, string secondPath)
        {
            using var reader1 = BinaryLogReplayEventSource.OpenBuildEventsReader(firstPath);
            using var reader2 = BinaryLogReplayEventSource.OpenBuildEventsReader(secondPath);

            Dictionary<string, string> embedFiles1 = new();
            Dictionary<string, string> embedFiles2 = new();

            reader1.ArchiveFileEncountered += arg
                => AddArchiveFile(embedFiles1, arg);

            reader2.ArchiveFileEncountered += arg
                => AddArchiveFile(embedFiles2, arg);

            int i = 0;
            while (reader1.Read() is { } ev1)
            {
                i++;
                var ev2 = reader2.Read();
                if (!Compare(ev1, ev2, out string diffReason, $"event arg {i}"))
                {
                    Assert.Fail($"Binlogs ({firstPath} and {secondPath}) are not equal at event {i} ({diffReason})");
                }
            }
            // Read the second reader - to confirm there are no more events
            //  and to force the embedded files to be read.
            reader2.Read().Should().BeNull($"Binlogs ({firstPath} and {secondPath}) are not equal - second has more events >{i + 1}");

            Assert.Equal(embedFiles1, embedFiles2);

            void AddArchiveFile(Dictionary<string, string> files, ArchiveFileEventArgs arg)
            {
                ArchiveFile embedFile = arg.ArchiveData.ToArchString();
                string content = embedFile.Content;
                files.Add(embedFile.FullPath, content);
            }
        }

        private static bool Compare(object? left, object? right, out string diffReason, string name = "", HashSet<object?>? compared = null)
        {
            diffReason = string.Empty;
            if (compared == null)
            {
                compared = new HashSet<object?>();
            }
            else if (compared.Contains(left) && compared.Contains(right))
            {
                return true;
            }
            else
            {
                compared.Add(left);
                compared.Add(right);
            }

            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((left == null) ^ (right == null))
            {
                diffReason = "One object is null and the other is not." + name;
                return false;
            }

            if (left!.GetType() != right!.GetType())
            {
                diffReason = $"Object types are different ({left.GetType().FullName} vs {right.GetType().FullName}).";
                return false;
            }

            Type type = left.GetType();
            if (name == string.Empty)
            {
                name = type.Name;
            }

            if (IsSimpleType(type))
            {
                if (!left.Equals(right))
                {
                    diffReason = $"Objects are different ({left} vs {right}). " + name;
                    return false;
                }
                return true;
            }

            if (type.IsArray)
            {
                Array first = (left as Array)!;
                Array second = (right as Array)!;
                if (first.Length != second.Length)
                {
                    diffReason = $"{type.Name} : array size differs ({first.Length} vs {second.Length})";
                    return false;
                }

                var en = first.GetEnumerator();
                int i = 0;
                while (en.MoveNext())
                {
                    if (!Compare(en.Current, second.GetValue(i), out diffReason, name, compared))
                    {
                        diffReason += $" (Index {i})";
                        return false;
                    }
                    i++;
                }
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                System.Collections.IEnumerable first = (left as System.Collections.IEnumerable)!;
                System.Collections.IEnumerable second = (right as System.Collections.IEnumerable)!;

                var en = first.GetEnumerator();
                var en2 = second.GetEnumerator();
                int i = 0;
                while (en.MoveNext())
                {
                    if (!en2.MoveNext())
                    {
                        diffReason = $"{name} : enumerable size differs";
                        return false;
                    }

                    if (!Compare(en.Current, en2.Current, out diffReason, name, compared))
                    {
                        diffReason += $" (Position {i})";
                        return false;
                    }
                    i++;
                }
            }
            else
            {
                // Careful - the default argument-less impl gets the static properties as well (e.g. DateTime.Now)
                foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    try
                    {
                        var val = pi.GetValue(left);
                        var tval = pi.GetValue(right);
                        var name1 = name + "." + pi.Name;
                        if (!Compare(val, tval, out diffReason, name1, compared))
                        {
                            return false;
                        }
                    }
                    catch (TargetParameterCountException)
                    {
                        // index property
                    }
                }
            }

            return true;
        }

        internal static bool IsSimpleType(Type type)
        {
            // Nullables
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsSimpleType(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
                   || type.IsEnum
                   || type == typeof(string)
                   || type == typeof(decimal);
        }
    }
}
