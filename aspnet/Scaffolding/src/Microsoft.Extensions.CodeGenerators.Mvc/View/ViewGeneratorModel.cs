// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CodeGeneration.CommandLine;

namespace Microsoft.Extensions.CodeGenerators.Mvc.View
{
    public class ViewGeneratorModel : CommonCommandLineModel
    {
        public string ViewName { get; set; }

        [Argument(Description = "The view template to use, supported view templates: 'Create|Edit|Delete|Details|List'")]
        public string TemplateName { get; set; }

        [Option(Name = "partialView", ShortName = "partial", Description = "Generate a partial view, other layout options (-l and -udl) are ignored if this is specified")]
        public bool PartialView { get; set; }
    }
}