// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.CodeGeneration
{
    public interface IPackageInstaller
    {
        Task InstallPackages(IEnumerable<PackageMetadata> packages);
    }
}