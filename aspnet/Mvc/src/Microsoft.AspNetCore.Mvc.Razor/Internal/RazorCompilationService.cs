// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IRazorCompilationService"/>.
    /// </summary>
    public class RazorCompilationService : IRazorCompilationService
    {
        private readonly ICompilationService _compilationService;
        private readonly IMvcRazorHost _razorHost;
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new instance of the <see cref="RazorCompilationService"/> class.
        /// </summary>
        /// <param name="compilationService">The <see cref="ICompilationService"/> to compile generated code.</param>
        /// <param name="razorHost">The <see cref="IMvcRazorHost"/> to generate code from Razor files.</param>
        /// <param name="fileProviderAccessor">The <see cref="IRazorViewEngineFileProviderAccessor"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public RazorCompilationService(
            ICompilationService compilationService,
            IMvcRazorHost razorHost,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            ILoggerFactory loggerFactory)
        {
            _compilationService = compilationService;
            _razorHost = razorHost;
            _fileProvider = fileProviderAccessor.FileProvider;
            _logger = loggerFactory.CreateLogger<RazorCompilationService>();
        }

        /// <inheritdoc />
        public CompilationResult Compile(RelativeFileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            GeneratorResults results;
            using (var inputStream = file.FileInfo.CreateReadStream())
            {
                _logger.RazorFileToCodeCompilationStart(file.RelativePath);

                var startTimestamp = _logger.IsEnabled(LogLevel.Debug) ? Stopwatch.GetTimestamp() : 0;

                results = GenerateCode(file.RelativePath, inputStream);

                _logger.RazorFileToCodeCompilationEnd(file.RelativePath, startTimestamp);
            }

            if (!results.Success)
            {
                return GetCompilationFailedResult(file, results.ParserErrors);
            }

            return _compilationService.Compile(file, results.GeneratedCode);
        }

        /// <summary>
        /// Generate code for the Razor file at <paramref name="relativePath"/> with content
        /// <paramref name="inputStream"/>.
        /// </summary>
        /// <param name="relativePath">
        /// The path of the Razor file relative to the root of the application. Used to generate line pragmas and
        /// calculate the class name of the generated type.
        /// </param>
        /// <param name="inputStream">A <see cref="Stream"/> that contains the Razor content.</param>
        /// <returns>A <see cref="GeneratorResults"/> instance containing results of code generation.</returns>
        protected virtual GeneratorResults GenerateCode(string relativePath, Stream inputStream)
        {
            return _razorHost.GenerateCode(relativePath, inputStream);
        }

        // Internal for unit testing
        internal CompilationResult GetCompilationFailedResult(RelativeFileInfo file, IEnumerable<RazorError> errors)
        {
            // If a SourceLocation does not specify a file path, assume it is produced
            // from parsing the current file.
            var messageGroups = errors
                .GroupBy(razorError =>
                razorError.Location.FilePath ?? file.RelativePath,
                StringComparer.Ordinal);

            var failures = new List<CompilationFailure>();
            foreach (var group in messageGroups)
            {
                var filePath = group.Key;
                var fileContent = ReadFileContentsSafely(filePath);
                var compilationFailure = new CompilationFailure(
                    filePath,
                    fileContent,
                    compiledContent: string.Empty,
                    messages: group.Select(parserError => CreateDiagnosticMessage(parserError, filePath)));
                failures.Add(compilationFailure);
            }

            return new CompilationResult(failures);
        }

        private DiagnosticMessage CreateDiagnosticMessage(RazorError error, string filePath)
        {
            var location = error.Location;
            return new DiagnosticMessage(
                message: error.Message,
                formattedMessage: $"{error} ({location.LineIndex},{location.CharacterIndex}) {error.Message}",
                filePath: filePath,
                startLine: error.Location.LineIndex + 1,
                startColumn: error.Location.CharacterIndex,
                endLine: error.Location.LineIndex + 1,
                endColumn: error.Location.CharacterIndex + error.Length);
        }

        private string ReadFileContentsSafely(string relativePath)
        {
            var fileInfo = _fileProvider.GetFileInfo(relativePath);
            if (fileInfo.Exists)
            {
                try
                {
                    using (var reader = new StreamReader(fileInfo.CreateReadStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch
                {
                    // Ignore any failures
                }
            }

            return null;
        }
    }
}
