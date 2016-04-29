﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CodeGeneration;
using Microsoft.Extensions.CodeGenerators.Mvc.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.CodeGeneration.DotNet;

namespace Microsoft.Extensions.CodeGenerators.Mvc.Controller
{
    /// <summary>
    /// MvcController class provides basic functionality for scaffolding an MVC controller. 
    /// The specific type of controller (Empty, Controller with read write actions etc, need to provide the template names to be used for scaffolding.
    /// </summary>
    public abstract class MvcController : ControllerGeneratorBase
    {
        public MvcController(
            ILibraryManager libraryManager,
            IApplicationInfo applicationInfo,
            ICodeGeneratorActionsService codeGeneratorActionsService,
            IServiceProvider serviceProvider,
            ILogger logger)
            : base(libraryManager, applicationInfo, codeGeneratorActionsService, serviceProvider, logger)
        {
        }
        public override async Task Generate(CommandLineGeneratorModel controllerGeneratorModel)
        {
            if (!string.IsNullOrEmpty(controllerGeneratorModel.ControllerName))
            {
                if (!controllerGeneratorModel.ControllerName.EndsWith(Constants.ControllerSuffix, StringComparison.Ordinal))
                {
                    controllerGeneratorModel.ControllerName = controllerGeneratorModel.ControllerName + Constants.ControllerSuffix;
                }
            }
            else
            {
                throw new ArgumentException(GetRequiredNameError);
            }

            var layoutDependencyInstaller = ActivatorUtilities.CreateInstance<MvcLayoutDependencyInstaller>(ServiceProvider);
            await layoutDependencyInstaller.Execute();

            var templateModel = new ClassNameModel(className: controllerGeneratorModel.ControllerName, namespaceName: GetControllerNamespace());

            var outputPath = ValidateAndGetOutputPath(controllerGeneratorModel);
            await CodeGeneratorActionsService.AddFileFromTemplateAsync(outputPath, GetTemplateName(controllerGeneratorModel), TemplateFolders, templateModel);
            Logger.LogMessage("Added Controller : " + outputPath.Substring(ApplicationInfo.ApplicationBasePath.Length));

            await layoutDependencyInstaller.InstallDependencies();
        }
        protected virtual string GetRequiredNameError
        {
            get
            {
                return CodeGenerators.Mvc.MessageStrings.ControllerNameRequired;
            }
        }
        
    }
}
