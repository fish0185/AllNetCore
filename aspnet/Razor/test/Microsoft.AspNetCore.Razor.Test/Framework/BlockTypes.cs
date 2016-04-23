// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.Parser.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Razor.Test.Framework
{
    // The product code doesn't need this, but having subclasses for the block types makes tests much cleaner :)

    public class StatementBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Statement;

        public StatementBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public StatementBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public StatementBlock(params SyntaxTreeNode[] children)
            : this(ParentChunkGenerator.Null, children)
        {
        }
    }

    public class DirectiveBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Directive;

        public DirectiveBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public DirectiveBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public DirectiveBlock(params SyntaxTreeNode[] children)
            : this(ParentChunkGenerator.Null, children)
        {
        }
    }

    public class FunctionsBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Functions;

        public FunctionsBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public FunctionsBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public FunctionsBlock(params SyntaxTreeNode[] children)
            : this(ParentChunkGenerator.Null, children)
        {
        }
    }

    public class ExpressionBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Expression;

        public ExpressionBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public ExpressionBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public ExpressionBlock(params SyntaxTreeNode[] children)
            : this(new ExpressionChunkGenerator(), children)
        {
        }
    }

    public class MarkupTagBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Tag;

        public MarkupTagBlock(params SyntaxTreeNode[] children)
            : base(ThisBlockType, children, ParentChunkGenerator.Null)
        {
        }
    }

    public class MarkupBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Markup;

        public MarkupBlock(
            BlockType blockType,
            IParentChunkGenerator chunkGenerator,
            IReadOnlyList<SyntaxTreeNode> children)
            : base(blockType, children, chunkGenerator)
        {
        }

        public MarkupBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : this(ThisBlockType, chunkGenerator, children)
        {
        }

        public MarkupBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public MarkupBlock(params SyntaxTreeNode[] children)
            : this(ParentChunkGenerator.Null, children)
        {
        }
    }

    public class MarkupTagHelperBlock : TagHelperBlock
    {
        public MarkupTagHelperBlock(string tagName)
            : this(tagName, tagMode: TagMode.StartTagAndEndTag, attributes: new List<KeyValuePair<string, SyntaxTreeNode>>())
        {
        }

        public MarkupTagHelperBlock(string tagName, TagMode tagMode)
            : this(tagName, tagMode, new List<KeyValuePair<string, SyntaxTreeNode>>())
        {
        }

        public MarkupTagHelperBlock(
            string tagName,
            IList<KeyValuePair<string, SyntaxTreeNode>> attributes)
            : this(tagName, TagMode.StartTagAndEndTag, attributes, children: new SyntaxTreeNode[0])
        {
        }

        public MarkupTagHelperBlock(
            string tagName,
            TagMode tagMode,
            IList<KeyValuePair<string, SyntaxTreeNode>> attributes)
            : this(tagName, tagMode, attributes, new SyntaxTreeNode[0])
        {
        }

        public MarkupTagHelperBlock(string tagName, params SyntaxTreeNode[] children)
            : this(
                  tagName,
                  TagMode.StartTagAndEndTag,
                  attributes: new List<KeyValuePair<string, SyntaxTreeNode>>(),
                  children: children)
        {
        }

        public MarkupTagHelperBlock(string tagName, TagMode tagMode, params SyntaxTreeNode[] children)
            : this(tagName, tagMode, new List<KeyValuePair<string, SyntaxTreeNode>>(), children)
        {
        }

        public MarkupTagHelperBlock(
            string tagName,
            IList<KeyValuePair<string, SyntaxTreeNode>> attributes,
            params SyntaxTreeNode[] children)
            : base(new TagHelperBlockBuilder(
                tagName,
                TagMode.StartTagAndEndTag,
                attributes: attributes,
                children: children))
        {
        }

        public MarkupTagHelperBlock(
            string tagName,
            TagMode tagMode,
            IList<KeyValuePair<string, SyntaxTreeNode>> attributes,
            params SyntaxTreeNode[] children)
            : base(new TagHelperBlockBuilder(tagName, tagMode, attributes, children))
        {
        }
    }

    public class SectionBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Section;

        public SectionBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public SectionBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public SectionBlock(params SyntaxTreeNode[] children)
            : this(ParentChunkGenerator.Null, children)
        {
        }

        public SectionBlock(IReadOnlyList<SyntaxTreeNode> children)
            : this(ParentChunkGenerator.Null, children)
        {
        }
    }

    public class TemplateBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Template;

        public TemplateBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public TemplateBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public TemplateBlock(params SyntaxTreeNode[] children)
            : this(new TemplateBlockChunkGenerator(), children)
        {
        }

        public TemplateBlock(IReadOnlyList<SyntaxTreeNode> children)
            : this(new TemplateBlockChunkGenerator(), children)
        {
        }
    }

    public class CommentBlock : Block
    {
        private const BlockType ThisBlockType = BlockType.Comment;

        public CommentBlock(IParentChunkGenerator chunkGenerator, IReadOnlyList<SyntaxTreeNode> children)
            : base(ThisBlockType, children, chunkGenerator)
        {
        }

        public CommentBlock(IParentChunkGenerator chunkGenerator, params SyntaxTreeNode[] children)
            : this(chunkGenerator, (IReadOnlyList<SyntaxTreeNode>)children)
        {
        }

        public CommentBlock(params SyntaxTreeNode[] children)
            : this(new RazorCommentChunkGenerator(), children)
        {
        }

        public CommentBlock(IReadOnlyList<SyntaxTreeNode> children)
            : this(new RazorCommentChunkGenerator(), children)
        {
        }
    }
}
