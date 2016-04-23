// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.AspNetCore.Testing
{
    public static class TestPlatformHelper
    {
        private static Lazy<IRuntimeEnvironment> _runtimeEnv = new Lazy<IRuntimeEnvironment>(
            () => PlatformServices.Default.Runtime);

        private static Lazy<bool> _isMono = new Lazy<bool>(
            () => RuntimeEnvironment.RuntimeType.Equals("Mono", StringComparison.OrdinalIgnoreCase));

        private static Lazy<bool> _isWindows = new Lazy<bool>(
            () => RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows);
        private static Lazy<bool> _isLinux = new Lazy<bool>(
            () => RuntimeEnvironment.OperatingSystemPlatform == Platform.Linux);
        private static Lazy<bool> _isMac = new Lazy<bool>(
            () => RuntimeEnvironment.OperatingSystemPlatform == Platform.Darwin);

        public static bool IsMono { get { return _isMono.Value; } }

        public static bool IsWindows { get { return _isWindows.Value; } }
        public static bool IsLinux { get { return _isLinux.Value; } }
        public static bool IsMac { get { return _isMac.Value; } }

        internal static IRuntimeEnvironment RuntimeEnvironment { get { return _runtimeEnv.Value; } }
    }
}