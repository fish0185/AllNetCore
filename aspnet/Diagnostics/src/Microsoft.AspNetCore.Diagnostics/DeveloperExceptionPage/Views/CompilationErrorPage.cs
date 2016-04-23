namespace Microsoft.AspNetCore.Diagnostics.Views
{
#line 1 "CompilationErrorPage.cshtml"
using System

#line default
#line hidden
    ;
#line 2 "CompilationErrorPage.cshtml"
using System.Globalization

#line default
#line hidden
    ;
#line 3 "CompilationErrorPage.cshtml"
using System.Linq

#line default
#line hidden
    ;
#line 4 "CompilationErrorPage.cshtml"
using System.Net

#line default
#line hidden
    ;
#line 5 "CompilationErrorPage.cshtml"
using Microsoft.AspNetCore.Diagnostics

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class CompilationErrorPage : Microsoft.AspNetCore.DiagnosticsViewPage.Views.BaseView
    {
#line 7 "CompilationErrorPage.cshtml"

    public CompilationErrorPageModel Model { get; set; }

#line default
#line hidden
        #line hidden
        public CompilationErrorPage()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
#line 10 "CompilationErrorPage.cshtml"
  
    Response.StatusCode = 500;
    Response.ContentType = "text/html; charset=utf-8";
    Response.ContentLength = null; // Clear any prior Content-Length

#line default
#line hidden

            WriteLiteral("<!DOCTYPE html>\r\n<html>\r\n    <head>\r\n        <meta charset=\"utf-8\" />\r\n        <title>");
#line 19 "CompilationErrorPage.cshtml"
          Write(Resources.ErrorPageHtml_Title);

#line default
#line hidden
            WriteLiteral("</title>\r\n        <style>\r\n            body {\r\n    font-family: 'Segoe UI', Tahoma, Arial, Helvetica, sans-serif;\r\n    font-size: .813em;\r\n    line-height: 1.4em;\r\n    color: #222;\r\n}\r\n\r\nh1, h2, h3, h4, h5 {\r\n    /*font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;*/\r\n    font-weight: 100;\r\n}\r\n\r\nh1 {\r\n    color: #44525e;\r\n    margin: 15px 0 15px 0;\r\n}\r\n\r\nh2 {\r\n    margin: 10px 5px 0 0;\r\n}\r\n\r\nh3 {\r\n    color: #363636;\r\n    margin: 5px 5px 0 0;\r\n}\r\n\r\ncode {\r\n    font-family: Consolas, \"Courier New\", courier, monospace;\r\n}\r\n\r\nbody .titleerror {\r\n    padding: 3px;\r\n    display: block;\r\n    font-size: 1.5em;\r\n    font-weight: 100;\r\n}\r\n\r\nbody .location {\r\n    margin: 3px 0 10px 30px;\r\n}\r\n\r\n#header {\r\n    font-size: 18px;\r\n    padding: 15px 0;\r\n    border-top: 1px #ddd solid;\r\n    border-bottom: 1px #ddd solid;\r\n    margin-bottom: 0;\r\n}\r\n\r\n    #header li {\r\n        display: inline;\r\n        margin: 5px;\r\n        padding: 5px;\r\n        color: #a0a0a0;\r\n        cursor: pointer;\r\n    }\r\n\r\n        #header li:hover {\r\n            background: #a9e4f9;\r\n            color: #fff;\r\n        }\r\n\r\n        #header .selected {\r\n            background: #44c5f2;\r\n            color: #fff;\r\n        }\r\n\r\n#stackpage ul {\r\n    list-style: none;\r\n    padding-left: 0;\r\n    margin: 0;\r\n    /*border-bottom: 1px #ddd solid;*/\r\n}\r\n\r\n#stackpage .stackerror {\r\n    padding: 5px;\r\n    border-bottom: 1px #ddd solid;\r\n}\r\n\r\n    #stackpage .stackerror:hover {\r\n        background-color: #f0f0f0;\r\n    }\r\n\r\n#stackpage .frame:hover {\r\n    background-color: #f0f0f0;\r\n    text-decoration: none;\r\n}\r\n\r\n#stackpage .frame {\r\n    padding: 2px;\r\n    margin: 0 0 0 30px;\r\n    border-bottom: 1px #ddd solid;\r\n    cursor: pointer;\r\n}\r\n\r\n    #stackpage .frame h3 {\r\n        padding: 5px;\r\n        margin: 0;\r\n    }\r\n\r\n#stackpage .source {\r\n    padding: 0;\r\n}\r\n\r\n    #stackpage .source ol li {\r\n        font-family: Consolas, \"Courier New\", courier, monospace;\r\n        white-space: pre;\r\n    }\r\n\r\n#stackpage .frame:hover .source .highlight li span {\r\n    color: #fff;\r\n    background: #b20000;\r\n}\r\n\r\n#stackpage .source ol.collapsible li {\r\n    color: #888;\r\n}\r\n\r\n    #stackpage .source ol.collapsible li span {\r\n        color: #606060;\r\n    }\r\n\r\n.page table {\r\n    border-collapse: separate;\r\n    border-spacing: 0;\r\n    margin: 0 0 20px;\r\n}\r\n\r\n.page th {\r\n    vertical-align: bottom;\r\n    padding: 10px 5px 5px 5px;\r\n    font-weight: 400;\r\n    color: #a0a0a0;\r\n    text-align: left;\r\n}\r\n\r\n.page td {\r\n    padding: 3px 10px;\r\n}\r\n\r\n.page th, .page td {\r\n    border-right: 1px #ddd solid;\r\n    border-bottom: 1px #ddd solid;\r\n    border-left: 1px transparent solid;\r\n    border-top: 1px transparent solid;\r\n    box-sizing: border-box;\r\n}\r\n\r\n    .page th:last-child, .page td:last-child {\r\n        border-right: 1px transparent solid;\r\n    }\r\n\r\n    .page .length {\r\n        text-align: right;\r\n    }\r\n\r\na {\r\n    color: #1ba1e2;\r\n    text-decoration: none;\r\n}\r\n\r\n    a:hover {\r\n        color: #13709e;\r\n        text-decoration: underline;\r\n    }\r\n\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <h1>");
#line 25 "CompilationErrorPage.cshtml"
       Write(Resources.ErrorPageHtml_CompilationException);

#line default
#line hidden
            WriteLiteral("</h1>\r\n");
#line 26 "CompilationErrorPage.cshtml"
        

#line default
#line hidden

#line 26 "CompilationErrorPage.cshtml"
         foreach (var errorDetail in Model.ErrorDetails)
        {

#line default
#line hidden

            WriteLiteral("            <div id=\"stackpage\" class=\"page\">\r\n");
#line 29 "CompilationErrorPage.cshtml"
                

#line default
#line hidden

#line 29 "CompilationErrorPage.cshtml"
                   int tabIndex = 6; 

#line default
#line hidden

            WriteLiteral("                ");
#line 30 "CompilationErrorPage.cshtml"
                  
                    var fileName = errorDetail.StackFrames.FirstOrDefault()?.File;
                    if (!string.IsNullOrEmpty(fileName))
                    {

#line default
#line hidden

            WriteLiteral("                        <div class=\"titleerror\">");
#line 34 "CompilationErrorPage.cshtml"
                                           Write(fileName);

#line default
#line hidden
            WriteLiteral("</div>\r\n");
#line 35 "CompilationErrorPage.cshtml"
                    }
                

#line default
#line hidden

            WriteLiteral("                <br />\r\n                <ul>\r\n");
#line 39 "CompilationErrorPage.cshtml"
                

#line default
#line hidden

#line 39 "CompilationErrorPage.cshtml"
                 foreach (var frame in errorDetail.StackFrames)
                {

#line default
#line hidden

            WriteLiteral("                    <li class=\"frame\"");
            BeginWriteAttribute("tabindex", " tabindex=\"", 1273, "\"", 1293, 1);
#line 41 "CompilationErrorPage.cshtml"
WriteAttributeValue("", 1284, tabIndex, 1284, 9, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n");
#line 42 "CompilationErrorPage.cshtml"
                        

#line default
#line hidden

#line 42 "CompilationErrorPage.cshtml"
                           tabIndex++; 

#line default
#line hidden

            WriteLiteral("                        ");
#line 43 "CompilationErrorPage.cshtml"
                         if (!string.IsNullOrEmpty(frame.ErrorDetails))
                        {

#line default
#line hidden

            WriteLiteral("                            <h3>");
#line 45 "CompilationErrorPage.cshtml"
                           Write(frame.ErrorDetails);

#line default
#line hidden
            WriteLiteral("</h3>\r\n");
#line 46 "CompilationErrorPage.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 48 "CompilationErrorPage.cshtml"
                        

#line default
#line hidden

#line 48 "CompilationErrorPage.cshtml"
                         if (frame.Line != 0 && frame.ContextCode.Any())
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"source\">\r\n");
#line 51 "CompilationErrorPage.cshtml"
                                

#line default
#line hidden

#line 51 "CompilationErrorPage.cshtml"
                                 if (frame.PreContextCode.Any())
                                {

#line default
#line hidden

            WriteLiteral("                                    <ol");
            BeginWriteAttribute("start", " start=\"", 1817, "\"", 1846, 1);
#line 53 "CompilationErrorPage.cshtml"
WriteAttributeValue("", 1825, frame.PreContextLine, 1825, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"collapsible\">\r\n");
#line 54 "CompilationErrorPage.cshtml"
                                        

#line default
#line hidden

#line 54 "CompilationErrorPage.cshtml"
                                         foreach (var line in frame.PreContextCode)
                                        {

#line default
#line hidden

            WriteLiteral("                                            <li><span>");
#line 56 "CompilationErrorPage.cshtml"
                                                 Write(line);

#line default
#line hidden
            WriteLiteral("</span></li>\r\n");
#line 57 "CompilationErrorPage.cshtml"
                                        }

#line default
#line hidden

            WriteLiteral("                                    </ol>\r\n");
#line 59 "CompilationErrorPage.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                <ol");
            BeginWriteAttribute("start", " start=\"", 2227, "\"", 2246, 1);
#line 60 "CompilationErrorPage.cshtml"
WriteAttributeValue("", 2235, frame.Line, 2235, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"highlight\">\r\n");
#line 61 "CompilationErrorPage.cshtml"
                                    

#line default
#line hidden

#line 61 "CompilationErrorPage.cshtml"
                                     foreach (var line in frame.ContextCode)
                                    {

#line default
#line hidden

            WriteLiteral("                                        <li><span>");
#line 63 "CompilationErrorPage.cshtml"
                                             Write(line);

#line default
#line hidden
            WriteLiteral("</span></li>\r\n");
#line 64 "CompilationErrorPage.cshtml"
                                    }

#line default
#line hidden

            WriteLiteral("                                </ol>\r\n");
#line 66 "CompilationErrorPage.cshtml"
                                

#line default
#line hidden

#line 66 "CompilationErrorPage.cshtml"
                                 if (frame.PostContextCode.Any())
                                {

#line default
#line hidden

            WriteLiteral("                                    <ol");
            BeginWriteAttribute("start", " start=\'", 2673, "\'", 2698, 1);
#line 68 "CompilationErrorPage.cshtml"
WriteAttributeValue("", 2681, frame.Line + 1, 2681, 17, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"collapsible\">\r\n");
#line 69 "CompilationErrorPage.cshtml"
                                        

#line default
#line hidden

#line 69 "CompilationErrorPage.cshtml"
                                         foreach (var line in frame.PostContextCode)
                                        {

#line default
#line hidden

            WriteLiteral("                                            <li><span>");
#line 71 "CompilationErrorPage.cshtml"
                                                 Write(line);

#line default
#line hidden
            WriteLiteral("</span></li>\r\n");
#line 72 "CompilationErrorPage.cshtml"
                                        }

#line default
#line hidden

            WriteLiteral("                                    </ol>\r\n");
#line 74 "CompilationErrorPage.cshtml"
                                } 

#line default
#line hidden

            WriteLiteral("                            </div>\r\n");
#line 76 "CompilationErrorPage.cshtml"
                        } 

#line default
#line hidden

            WriteLiteral("                    </li>\r\n");
#line 78 "CompilationErrorPage.cshtml"
                }

#line default
#line hidden

            WriteLiteral("                </ul>\r\n                <br />\r\n            </div>\r\n");
#line 82 "CompilationErrorPage.cshtml"
        }

#line default
#line hidden

            WriteLiteral("        <script>\r\n            //<!--\r\n            (function (window, undefined) {\r\n    \"use strict\";\r\n\r\n    function ns(selector, element) {\r\n        return new NodeCollection(selector, element);\r\n    }\r\n\r\n    function NodeCollection(selector, element) {\r\n        this.items = [];\r\n        element = element || window.document;\r\n\r\n        var nodeList;\r\n\r\n        if (typeof (selector) === \"string\") {\r\n            nodeList = element.querySelectorAll(selector);\r\n            for (var i = 0, l = nodeList.length; i < l; i++) {\r\n                this.items.push(nodeList.item(i));\r\n            }\r\n        }\r\n    }\r\n\r\n    NodeCollection.prototype = {\r\n        each: function (callback) {\r\n            for (var i = 0, l = this.items.length; i < l; i++) {\r\n                callback(this.items[i], i);\r\n            }\r\n            return this;\r\n        },\r\n\r\n        children: function (selector) {\r\n            var children = [];\r\n\r\n            this.each(function (el) {\r\n                children = children.concat(ns(selector, el).items);\r\n            });\r\n\r\n            return ns(children);\r\n        },\r\n\r\n        hide: function () {\r\n            this.each(function (el) {\r\n                el.style.display = \"none\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        toggle: function () {\r\n            this.each(function (el) {\r\n                el.style.display = el.style.display === \"none\" ? \"\" : \"none\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        show: function () {\r\n            this.each(function (el) {\r\n                el.style.display = \"\";\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        addClass: function (className) {\r\n            this.each(function (el) {\r\n                var existingClassName = el.className,\r\n                    classNames;\r\n                if (!existingClassName) {\r\n                    el.className = className;\r\n                } else {\r\n                    classNames = existingClassName.split(\" \");\r\n                    if (classNames.indexOf(className) < 0) {\r\n                        el.className = existingClassName + \" \" + className;\r\n                    }\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        removeClass: function (className) {\r\n            this.each(function (el) {\r\n                var existingClassName = el.className,\r\n                    classNames, index;\r\n                if (existingClassName === className) {\r\n                    el.className = \"\";\r\n                } else if (existingClassName) {\r\n                    classNames = existingClassName.split(\" \");\r\n                    index = classNames.indexOf(className);\r\n                    if (index > 0) {\r\n                        classNames.splice(index, 1);\r\n                        el.className = classNames.join(\" \");\r\n                    }\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        attr: function (name) {\r\n            if (this.items.length === 0) {\r\n                return null;\r\n            }\r\n\r\n            return this.items[0].getAttribute(name);\r\n        },\r\n\r\n        on: function (eventName, handler) {\r\n            this.each(function (el, idx) {\r\n                var callback = function (e) {\r\n                    e = e || window.event;\r\n                    if (!e.which && e.keyCode) {\r\n                        e.which = e.keyCode; // Normalize IE8 key events\r\n                    }\r\n                    handler.apply(el, [e]);\r\n                };\r\n\r\n                if (el.addEventListener) { // DOM Events\r\n                    el.addEventListener(eventName, callback, false);\r\n                } else if (el.attachEvent) { // IE8 events\r\n                    el.attachEvent(\"on\" + eventName, callback);\r\n                } else {\r\n                    el[\"on\" + type] = callback;\r\n                }\r\n            });\r\n\r\n            return this;\r\n        },\r\n\r\n        click: function (handler) {\r\n            return this.on(\"click\", handler);\r\n        },\r\n\r\n        keypress: function (handler) {\r\n            return this.on(\"keypress\", handler);\r\n        }\r\n    };\r\n\r\n    function frame(el) {\r\n        ns(\".source .collapsible\", el).toggle();\r\n    }\r\n\r\n    function tab(el) {\r\n        var unselected = ns(\"#header .selected\").removeClass(\"selected\").attr(\"id\");\r\n        var selected = ns(\"#\" + el.id).addClass(\"selected\").attr(\"id\");\r\n\r\n        ns(\"#\" + unselected + \"page\").hide();\r\n        ns(\"#\" + selected + \"page\").show();\r\n    }\r\n\r\n    ns(\".collapsible\").hide();\r\n    ns(\".page\").hide();\r\n    ns(\"#stackpage\").show();\r\n\r\n    ns(\"#rawExceptionButton\").click(function (event) {\r\n        var div = document.getElementById('rawException');\r\n        div.style.display = 'inline-block';\r\n        div.scrollIntoView(true);\r\n        event.preventDefault();\r\n        event.stopPropagation();\r\n\r\n        return false;\r\n    });\r\n\r\n    ns(\".frame\")\r\n        .click(function () {\r\n            frame(this);\r\n        })\r\n        .keypress(function (e) {\r\n            if (e.which === 13) {\r\n                frame(this);\r\n            }\r\n        });\r\n\r\n    ns(\"#header li\")\r\n        .click(function () {\r\n            tab(this);\r\n        })\r\n        .keypress(function (e) {\r\n            if (e.which === 13) {\r\n                tab(this);\r\n            }\r\n        });\r\n})(window);\r\n            //-->\r\n        </script>\r\n    </body>\r\n</html>\r\n");
        }
        #pragma warning restore 1998
    }
}
