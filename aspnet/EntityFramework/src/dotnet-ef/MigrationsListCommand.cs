﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class MigrationsListCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "List the migrations";

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var startupProject = command.Option(
                "-s|--startup-project <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var json = command.Option(
                "--json",
                "Use json output");
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(
                    context.Value(),
                    startupProject.Value(),
                    environment.Value(),
                    json.HasValue()
                        ? (Action<IEnumerable<IDictionary>>)ReportJsonResults
                        : ReportResults));
        }

        private static int Execute(
            string context,
            string startupProject,
            string environment,
            Action<IEnumerable<IDictionary>> reportResultsAction)
        {
            var migrations = new ReflectionOperationExecutor(startupProject, environment)
                .GetMigrations(context);

            reportResultsAction(migrations);

            return 0;
        }

        private static void ReportJsonResults(IEnumerable<IDictionary> migrations)
        {
            Reporter.Output.Write("[");

            var first = true;
            foreach (var migration in migrations)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Reporter.Output.Write(",");
                }

                Reporter.Output.WriteLine();
                Reporter.Output.WriteLine("  {");
                Reporter.Output.WriteLine("    \"id\": \"" + migration["Id"] + "\",");
                Reporter.Output.WriteLine("    \"name\": \"" + migration["Name"] + "\"");
                Reporter.Output.Write("  }");
            }

            Reporter.Output.WriteLine();
            Reporter.Output.WriteLine("]");
        }

        private static void ReportResults(IEnumerable<IDictionary> migrations)
        {
            var any = false;
            foreach (var migration in migrations)
            {
                Reporter.Output.WriteLine(migration["Id"] as string);
                any = true;
            }

            if (!any)
            {
                Reporter.Error.WriteLine("No migrations were found");
            }
        }
    }
}