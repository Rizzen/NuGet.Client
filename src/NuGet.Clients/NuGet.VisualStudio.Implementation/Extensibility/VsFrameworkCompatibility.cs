// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Versioning;
using NuGet.Frameworks;
using NuGet.VisualStudio.Implementation.Resources;

namespace NuGet.VisualStudio
{
    [Export(typeof(IVsFrameworkCompatibility))]
    [Export(typeof(IVsFrameworkCompatibility2))]
    [Export(typeof(IVsFrameworkCompatibility3))]
    public class VsFrameworkCompatibility : IVsFrameworkCompatibility2, IVsFrameworkCompatibility3
    {
        public IEnumerable<FrameworkName> GetNetStandardFrameworks()
        {
            return DefaultFrameworkNameProvider
                .Instance
                .GetNetStandardVersions()
                .Select(framework => new FrameworkName(framework.DotNetFrameworkName));
        }

        public IEnumerable<FrameworkName> GetFrameworksSupportingNetStandard(FrameworkName frameworkName)
        {
            if (frameworkName == null)
            {
                throw new ArgumentNullException(nameof(frameworkName));
            }

            var nuGetFramework = NuGetFramework.ParseFrameworkName(frameworkName.ToString(), DefaultFrameworkNameProvider.Instance);

            if (!StringComparer.OrdinalIgnoreCase.Equals(
                nuGetFramework.Framework,
                FrameworkConstants.FrameworkIdentifiers.NetStandard))
            {
                throw new ArgumentException(string.Format(
                    VsResources.InvalidNetStandardFramework,
                    frameworkName));
            }

            return CompatibilityListProvider
                .Default
                .GetFrameworksSupporting(nuGetFramework)
                .Select(framework => new FrameworkName(framework.DotNetFrameworkName));
        }

        public FrameworkName GetNearest(FrameworkName targetFramework, IEnumerable<FrameworkName> frameworks)
        {
            return GetNearest(targetFramework, Enumerable.Empty<FrameworkName>(), frameworks);
        }

        public FrameworkName GetNearest(FrameworkName targetFramework, IEnumerable<FrameworkName> fallbackTargetFrameworks, IEnumerable<FrameworkName> frameworks)
        {
            if (targetFramework == null)
            {
                throw new ArgumentNullException(nameof(targetFramework));
            }

            if (fallbackTargetFrameworks == null)
            {
                throw new ArgumentNullException(nameof(fallbackTargetFrameworks));
            }

            if (frameworks == null)
            {
                throw new ArgumentNullException(nameof(frameworks));
            }

            var nuGetTargetFramework = NuGetFramework.ParseFrameworkName(targetFramework.ToString(), DefaultFrameworkNameProvider.Instance);

            var nuGetFallbackTargetFrameworks = fallbackTargetFrameworks
                .Select(framework => NuGetFramework.ParseFrameworkName(framework.ToString(), DefaultFrameworkNameProvider.Instance))
                .ToList();

            if (nuGetFallbackTargetFrameworks.Any())
            {
                nuGetTargetFramework = new FallbackFramework(nuGetTargetFramework, nuGetFallbackTargetFrameworks);
            }

            var nuGetFrameworks = frameworks
                .Select(framework => NuGetFramework.ParseFrameworkName(framework.ToString(), DefaultFrameworkNameProvider.Instance));

            var reducer = new FrameworkReducer();
            var nearest = reducer.GetNearest(nuGetTargetFramework, nuGetFrameworks);

            if (nearest == null)
            {
                return null;
            }

            return new FrameworkName(nearest.DotNetFrameworkName);
        }

        public string GetNearest(string targetFramework, IEnumerable<string> frameworks)
        {
            return GetNearest(targetFramework, Enumerable.Empty<string>(), frameworks);
        }

        public string GetNearest(string targetFramework, IEnumerable<string> fallbackTargetFrameworks, IEnumerable<string> frameworks)
        {
            if (targetFramework == null)
            {
                throw new ArgumentNullException(nameof(targetFramework));
            }

            if (fallbackTargetFrameworks == null)
            {
                throw new ArgumentNullException(nameof(fallbackTargetFrameworks));
            }

            if (frameworks == null)
            {
                throw new ArgumentNullException(nameof(frameworks));
            }

            Dictionary<NuGetFramework, string> originalStrings = new Dictionary<NuGetFramework, string>();

            NuGetFramework ParseFrameworkString(string framework, string paramName)
            {
                NuGetFramework nugetFramework = NuGetFramework.Parse(framework);
                if (!nugetFramework.IsSpecificFramework)
                {
                    throw new ArgumentException($"Framework '{framework}' could not be parsed", paramName);
                }
                originalStrings[nugetFramework] = framework;
                return nugetFramework;
            }

            List<NuGetFramework> ParseFrameworkEnumerable(IEnumerable<string> enumerable, string paramName)
            {
                var list = new List<NuGetFramework>();
                foreach (var framework in enumerable)
                {
                    if (framework == null)
                    {
                        throw new ArgumentException("Enumeration contains a null value", paramName);
                    }
                    NuGetFramework nugetFramework = ParseFrameworkString(framework, paramName);
                    list.Add(nugetFramework);
                }
                return list;
            }

            NuGetFramework targetNuGetFramework = ParseFrameworkString(targetFramework, nameof(targetFramework));
            List<NuGetFramework> nugetFallbackTargetFrameworks = ParseFrameworkEnumerable(fallbackTargetFrameworks, nameof(fallbackTargetFrameworks));
            List<NuGetFramework> nugetFrameworks = ParseFrameworkEnumerable(frameworks, nameof(frameworks));

            if (nugetFallbackTargetFrameworks.Count > 0)
            {
                targetNuGetFramework = new FallbackFramework(targetNuGetFramework, nugetFallbackTargetFrameworks);
            }

            var reducer = new FrameworkReducer();
            var nearest = reducer.GetNearest(targetNuGetFramework, nugetFrameworks);

            if (nearest == null)
            {
                return null;
            }

            var originalFrameworkString = originalStrings[nearest];
            return originalFrameworkString;
        }
    }
}
