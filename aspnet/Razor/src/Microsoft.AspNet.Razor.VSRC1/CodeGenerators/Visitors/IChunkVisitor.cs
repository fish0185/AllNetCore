// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Chunks;

namespace Microsoft.AspNet.Razor.CodeGenerators.Visitors
{
    public interface IChunkVisitor
    {
        void Accept(IList<Chunk> chunks);
        void Accept(Chunk chunk);
    }
}
