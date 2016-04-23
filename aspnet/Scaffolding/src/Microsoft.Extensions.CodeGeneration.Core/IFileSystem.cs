// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.CodeGeneration
{
    internal interface IFileSystem
    {
        bool FileExists(string path);

        bool DirectoryExists(string path);

        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

        void MakeFileWritable(string path);

        string ReadAllText(string path);

        void WriteAllText(string path, string contents);

        Task AddFileAsync(string outputPath, Stream sourceStream);

        void CreateDirectory(string path);
    }
}