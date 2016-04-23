﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.Extensions.CodeGeneration.DotNet;
using Xunit;

namespace Microsoft.Extensions.CodeGeneration.Sources.Test
{
    public class LibraryExporterTests : TestBase
    {

        LibraryExporter _libraryExporter;

        public LibraryExporterTests()
            : base (Path.Combine("..", "TestApps", "ModelTypesLocatorTestClassLibrary"))
        {
            _libraryExporter = new LibraryExporter(_projectContext, _environment);
        }

        [Fact]
        public void LibraryExporter_TestGetAllExports()
        {
            _libraryExporter = new LibraryExporter(_projectContext, _environment);

            var exports = _libraryExporter.GetAllExports();

            Assert.True(exports.Any(), "No exports found");
            Assert.True(exports.First().Library.Identity.Type.Value == "Project");
            Assert.True(exports.First().Library.Identity.Name == "ModelTypesLocatorTestClassLibrary");
            Assert.True(exports.Where(_ => _.Library.Identity.Name == "Microsoft.Extensions.CodeGenerators.Mvc").Any());
        }

        [Fact]
        public void LibraryExporter_TestGetExport()
        {
            var export = _libraryExporter.GetExport("Microsoft.Extensions.CodeGenerators.Mvc");
            Assert.Equal("Microsoft.Extensions.CodeGenerators.Mvc", export.Library.Identity.Name);
        }

        [Fact]
        public void LibraryExporter_TestGetProjectsInApp()
        {
            var projecsInApp = _libraryExporter.GetProjectsInApp();

            //Assert.Equal(1, projecsInApp.Count());
            Assert.Equal("ModelTypesLocatorTestClassLibrary", projecsInApp.First().Library.Identity.Name);
        }

        [Fact]
        public void LibraryExporter_TestGetResolvedPath()
        {
            //Arrange
            LibraryManager manager = new LibraryManager(_projectContext);
            var lib = manager.GetLibrary("Microsoft.Extensions.CodeGenerators.Mvc");
            //Act
            var path = _libraryExporter.GetResolvedPathForDependency(lib);

            //Assert
            Assert.True(File.Exists(path));
        }

    }
}
