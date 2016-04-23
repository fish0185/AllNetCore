﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.CodeGeneration.DotNet
{
    public interface ICodeGenAssemblyLoadContext
    {
        Assembly LoadFromPath(AssemblyName assemblyName, string path);
        Assembly LoadFromPath(string path);
        Assembly LoadStream(Stream assembly, Stream symbols);
        Assembly LoadFromName(AssemblyName AssemblyName);
    }
}
