// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Collections.Generic;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    /// <summary>
    /// marker macro for blocks of BML code (embeded pythonized html/xml)
    /// processed by special pipeline step
    /// </summary>
    public partial class BmlMacro : LexicalInfoPreservingMacro{
        public static IDictionary<string, IDictionary<string, string>>
            DefaultAttributes = new Dictionary<string, IDictionary<string, string>>{
                                                                                       {
                                                                                           "style",
                                                                                           new Dictionary
                                                                                           <string, string>{
                                                                                                               {
                                                                                                                   "type",
                                                                                                                   "text/css"
                                                                                                                   },
                                                                                                           }
                                                                                           },
                                                                                       {
                                                                                           "script",
                                                                                           new Dictionary
                                                                                           <string, string>{
                                                                                                               {
                                                                                                                   "type",
                                                                                                                   "text/javascript"
                                                                                                                   },
                                                                                                           }
                                                                                           }
                                                                                   };

        public static string[] SupportedElements = new[]{
                                                            "div",
                                                            "span",
                                                            "p",
                                                            "h1",
                                                            "h2",
                                                            "h3",
                                                            "h4",
                                                            "h5",
                                                            "br",
                                                            "hr",
                                                            "style",
                                                            "script",
                                                            "img",
                                                            "a",
                                                            "table",
                                                            "td",
                                                            "th",
                                                            "tr",
															"col",
															"colgroup",
                                                            "thead",
                                                            "tbody",
                                                            "ul",
                                                            "ol",
                                                            "li",
                                                            "br",
                                                            "hr",
                                                            "strong",
                                                            "b",
                                                            "i",
                                                            "u",
                                                            "object",
                                                            "embed",
                                                            "html",
                                                            "head",
                                                            "body",
                                                            "form",
                                                            "input",
                                                            "link",
                                                            "meta",
                                                            "select",
                                                            "option",
                                                            "optgroup",
                                                            "textarea",
                                                            "bmlempty",
															"article",
															"section",
															"nav",
															"aside",
															"header",
															"footer",
															"hgroup",
															"dl",
															"dt",
															"dd",
                                                        };

        protected override Statement ExpandImpl(MacroStatement macro){
            return macro.Body;
        }
    }
}