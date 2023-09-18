// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VerifyTests.DiffPlex;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    internal static class VerifyInitialization
    {
        internal const string SnapshotsDirectory = "Snapshots";

        [ModuleInitializer]
        public static void Initialize()
        {
            DerivePathInfo(
                (_, _, type, method) => new(
                    directory: SnapshotsDirectory,
                    typeName: type.Name,
                    methodName: method.Name));

            // Customize diff output of verifier
            VerifyDiffPlex.Initialize(OutputType.Compact);
        }
    }
}
