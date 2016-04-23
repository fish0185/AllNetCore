﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class MigrationsRemoveCommand
    {
        public static void Configure([NotNull] CommandLineApplication command)
        {
            command.Description = "Remove the last migration";

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var startupProject = command.Option(
                "-s|--startup-project <project>",
                "The startup project to use. If omitted, the current project is used.");
            var environment = command.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.");
            var force = command.Option(
                "-f|--force",
                "Removes the last migration without checking the database. If the last migration has been applied to the database, you will need to manually reverse the changes it made.",
                CommandOptionType.NoValue);
            command.HelpOption();
            command.VerboseOption();

            command.OnExecute(
                () => Execute(
                    context.Value(),
                    startupProject.Value(),
                    environment.Value(),
                    force.HasValue()));
        }

        private static int Execute(string context, string startupProject, string environment, bool force)
        {
            new ReflectionOperationExecutor(startupProject, environment)
                .RemoveMigration(context, force);

            return 0;
        }
    }
}