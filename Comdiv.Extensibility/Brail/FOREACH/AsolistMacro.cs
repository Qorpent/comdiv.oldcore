// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE


using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public partial class ForeachMacro{
        #region Nested type: AsolistMacro

        public class AsolistMacro : ForeachSnippet<AsolistMacro>{
            public AsolistMacro(){
                SkipBinaryParametersOnGetAttributes = new[]{"li"};
                beforeall("<ol#ATTR>")
                    .beforeeach("<li>")
                    .aftereach("</li>")
                    .afterall("</ol>");
            }

            protected override object onbeforeeach(object realexpression, Expression expression,
                                                   MacroStatement statement){
                return getAttributedTagFromMacroExtensionParameters(statement, "li");
            }
        }

        #endregion
    }
}