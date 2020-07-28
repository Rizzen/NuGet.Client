// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.VisualStudio
{
    internal class VsNuGetFramework : IVsNuGetFramework
    {
        public VsNuGetFramework(
            string targetFrameworkIdentifier,
            Version targetFrameworkVersion,
            string profile,
            string targetPlatformIdentifier,
            Version targetPlatformVersion)
        {
            TargetFrameworkIdentifier = targetFrameworkIdentifier ?? throw new ArgumentNullException(nameof(targetFrameworkIdentifier));
            TargetFrameworkVersion = targetFrameworkVersion ?? throw new ArgumentNullException(nameof(targetFrameworkVersion));
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            TargetPlatformIdentifier = targetPlatformIdentifier ?? throw new ArgumentNullException(nameof(targetPlatformIdentifier));
            TargetPlatformVersion = targetPlatformVersion ?? throw new ArgumentNullException(nameof(targetPlatformVersion));
        }

        public string TargetFrameworkIdentifier { get; }

        public Version TargetFrameworkVersion { get; }

        public string Profile { get; }

        public string TargetPlatformIdentifier { get; }

        public Version TargetPlatformVersion { get; }
    }
}
