﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Graph;

namespace Microsoft.Extensions.CodeGeneration.DotNet
{
    public class LibraryExporter : ILibraryExporter
    {
        private Microsoft.DotNet.ProjectModel.Compilation.LibraryExporter _libraryExporter;
        private ProjectContext _context;

        public LibraryExporter(ProjectContext context, IApplicationInfo applicationInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            _libraryExporter = context.CreateExporter(applicationInfo.ApplicationConfiguration);
            _context = context;
        }

        public IEnumerable<LibraryExport> GetAllExports()
        {
            return _libraryExporter.GetAllExports();
        }

        public LibraryExport GetExport(string name)
        {
            return _libraryExporter.GetAllExports().Where(_ => _.Library.Identity.Name == name).FirstOrDefault();
        }

        public IEnumerable<LibraryExport> GetProjectsInApp()
        {
            return _libraryExporter.GetAllExports().Where(_ => _.Library.Identity.Type == LibraryType.Project);
        }
    }
}
