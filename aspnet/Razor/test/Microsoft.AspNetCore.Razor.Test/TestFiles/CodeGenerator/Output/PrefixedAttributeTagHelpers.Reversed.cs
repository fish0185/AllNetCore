#pragma checksum "PrefixedAttributeTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5ae668a393146e4a06179eb37952603907a9b825"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class PrefixedAttributeTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private global::Microsoft.AspNetCore.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = null;
        private global::TestNamespace.InputTagHelper2 __TestNamespace_InputTagHelper2 = null;
        private global::TestNamespace.InputTagHelper1 __TestNamespace_InputTagHelper1 = null;
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", new global::Microsoft.AspNetCore.Html.HtmlEncodedString("checkbox"));
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", new global::Microsoft.AspNetCore.Html.HtmlEncodedString("password"));
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", new global::Microsoft.AspNetCore.Html.HtmlEncodedString("radio"));
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("int-prefix-salt", new global::Microsoft.AspNetCore.Html.HtmlEncodedString("8"));
        #line hidden
        public PrefixedAttributeTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelperRunner();
            __tagHelperScopeManager = __tagHelperScopeManager ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
            Instrumentation.BeginContext(31, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
#line 3 "PrefixedAttributeTagHelpers.cshtml"
  
    var literate = "or illiterate";
    var intDictionary = new Dictionary<string, int>
    {
        { "three", 3 },
    };
    var stringDictionary = new SortedDictionary<string, string>
    {
        { "name", "value" },
    };

#line default
#line hidden

            Instrumentation.BeginContext(280, 49, true);
            WriteLiteral("\r\n<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper2);
            __TestNamespace_InputTagHelper1 = CreateTagHelper<global::TestNamespace.InputTagHelper1>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
#line 16 "PrefixedAttributeTagHelpers.cshtml"
__TestNamespace_InputTagHelper2.IntDictionaryProperty = intDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-dictionary", __TestNamespace_InputTagHelper2.IntDictionaryProperty);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty = __TestNamespace_InputTagHelper2.IntDictionaryProperty;
#line 16 "PrefixedAttributeTagHelpers.cshtml"
                  __TestNamespace_InputTagHelper2.StringDictionaryProperty = stringDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("string-dictionary", __TestNamespace_InputTagHelper2.StringDictionaryProperty);
            __TestNamespace_InputTagHelper1.StringDictionaryProperty = __TestNamespace_InputTagHelper2.StringDictionaryProperty;
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(329, 92, false);
            Write(__tagHelperExecutionContext.Output);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(421, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper2);
            if (__TestNamespace_InputTagHelper2.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-garlic", "TestNamespace.InputTagHelper2", "IntDictionaryProperty"));
            }
            __TestNamespace_InputTagHelper1 = CreateTagHelper<global::TestNamespace.InputTagHelper1>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper1);
            if (__TestNamespace_InputTagHelper1.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-garlic", "TestNamespace.InputTagHelper1", "IntDictionaryProperty"));
            }
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
#line 17 "PrefixedAttributeTagHelpers.cshtml"
__TestNamespace_InputTagHelper2.IntDictionaryProperty = intDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-dictionary", __TestNamespace_InputTagHelper2.IntDictionaryProperty);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty = __TestNamespace_InputTagHelper2.IntDictionaryProperty;
#line 17 "PrefixedAttributeTagHelpers.cshtml"
           __TestNamespace_InputTagHelper2.IntDictionaryProperty["garlic"] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-garlic", __TestNamespace_InputTagHelper2.IntDictionaryProperty["garlic"]);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty["garlic"] = __TestNamespace_InputTagHelper2.IntDictionaryProperty["garlic"];
#line 17 "PrefixedAttributeTagHelpers.cshtml"
                                  __TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"] = 42;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-grabber", __TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"]);
            __TestNamespace_InputTagHelper1.IntProperty = __TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"];
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(427, 103, false);
            Write(__tagHelperExecutionContext.Output);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(530, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper2);
            if (__TestNamespace_InputTagHelper2.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-grabber", "TestNamespace.InputTagHelper2", "IntDictionaryProperty"));
            }
            if (__TestNamespace_InputTagHelper2.StringDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("string-prefix-grabber", "TestNamespace.InputTagHelper2", "StringDictionaryProperty"));
            }
            __TestNamespace_InputTagHelper1 = CreateTagHelper<global::TestNamespace.InputTagHelper1>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper1);
            if (__TestNamespace_InputTagHelper1.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-salt", "TestNamespace.InputTagHelper1", "IntDictionaryProperty"));
            }
            if (__TestNamespace_InputTagHelper1.StringDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("string-prefix-paprika", "TestNamespace.InputTagHelper1", "StringDictionaryProperty"));
            }
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
#line 19 "PrefixedAttributeTagHelpers.cshtml"
__TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"] = 42;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-grabber", __TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"]);
            __TestNamespace_InputTagHelper1.IntProperty = __TestNamespace_InputTagHelper2.IntDictionaryProperty["grabber"];
#line 19 "PrefixedAttributeTagHelpers.cshtml"
__TestNamespace_InputTagHelper2.IntDictionaryProperty["salt"] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-salt", __TestNamespace_InputTagHelper2.IntDictionaryProperty["salt"]);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty["salt"] = __TestNamespace_InputTagHelper2.IntDictionaryProperty["salt"];
#line 19 "PrefixedAttributeTagHelpers.cshtml"
         __TestNamespace_InputTagHelper2.IntDictionaryProperty["pepper"] = 98;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-pepper", __TestNamespace_InputTagHelper2.IntDictionaryProperty["pepper"]);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty["pepper"] = __TestNamespace_InputTagHelper2.IntDictionaryProperty["pepper"];
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
            __TestNamespace_InputTagHelper2.StringDictionaryProperty["grabber"] = "string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-grabber", __TestNamespace_InputTagHelper2.StringDictionaryProperty["grabber"]);
            __TestNamespace_InputTagHelper1.StringProperty = __TestNamespace_InputTagHelper2.StringDictionaryProperty["grabber"];
            __TestNamespace_InputTagHelper2.StringDictionaryProperty["paprika"] = "another string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-paprika", __TestNamespace_InputTagHelper2.StringDictionaryProperty["paprika"]);
            __TestNamespace_InputTagHelper1.StringDictionaryProperty["paprika"] = __TestNamespace_InputTagHelper2.StringDictionaryProperty["paprika"];
            StartTagHelperWritingScope(null);
            WriteLiteral("literate ");
#line 21 "PrefixedAttributeTagHelpers.cshtml"
                             WriteLiteral(literate);

#line default
#line hidden
            WriteLiteral("?");
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __TestNamespace_InputTagHelper2.StringDictionaryProperty["cumin"] = __tagHelperStringValueBuffer.GetContent(HtmlEncoder);
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-cumin", __TestNamespace_InputTagHelper2.StringDictionaryProperty["cumin"]);
            __TestNamespace_InputTagHelper1.StringDictionaryProperty["cumin"] = __TestNamespace_InputTagHelper2.StringDictionaryProperty["cumin"];
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(536, 257, false);
            Write(__tagHelperExecutionContext.Output);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(793, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper2);
            if (__TestNamespace_InputTagHelper2.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-value", "TestNamespace.InputTagHelper2", "IntDictionaryProperty"));
            }
            if (__TestNamespace_InputTagHelper2.StringDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("string-prefix-thyme", "TestNamespace.InputTagHelper2", "StringDictionaryProperty"));
            }
            __TestNamespace_InputTagHelper1 = CreateTagHelper<global::TestNamespace.InputTagHelper1>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper1);
            if (__TestNamespace_InputTagHelper1.IntDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("int-prefix-value", "TestNamespace.InputTagHelper1", "IntDictionaryProperty"));
            }
            if (__TestNamespace_InputTagHelper1.StringDictionaryProperty == null)
            {
                throw new InvalidOperationException(FormatInvalidIndexerAssignment("string-prefix-thyme", "TestNamespace.InputTagHelper1", "StringDictionaryProperty"));
            }
#line 22 "PrefixedAttributeTagHelpers.cshtml"
__TestNamespace_InputTagHelper2.IntDictionaryProperty["value"] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-value", __TestNamespace_InputTagHelper2.IntDictionaryProperty["value"]);
            __TestNamespace_InputTagHelper1.IntDictionaryProperty["value"] = __TestNamespace_InputTagHelper2.IntDictionaryProperty["value"];
            __TestNamespace_InputTagHelper2.StringDictionaryProperty["thyme"] = "string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-thyme", __TestNamespace_InputTagHelper2.StringDictionaryProperty["thyme"]);
            __TestNamespace_InputTagHelper1.StringDictionaryProperty["thyme"] = __TestNamespace_InputTagHelper2.StringDictionaryProperty["thyme"];
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(799, 60, false);
            Write(__tagHelperExecutionContext.Output);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(859, 8, true);
            WriteLiteral("\r\n</div>");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
