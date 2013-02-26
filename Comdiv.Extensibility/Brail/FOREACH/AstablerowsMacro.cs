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


using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public partial class ForeachMacro{
        #region Nested type: AstablerowsMacro

        public class
            AstablerowsMacro : ForeachSnippet<AstablerowsMacro>{
            private IList<CellDefinition> cells;

            public override Statement Expand(MacroStatement macro){
                cells = macro.get<IList<CellDefinition>>("_cells");
                if (cells != null){
                    Expression trstart = "<tr>".toLiteral();
                    var trstart_ =
                        ((MethodInvocationExpression) getAttributedTagFromMacroExtensionParameters(macro, "row", "tr"));
                    if (trstart_ != null){
                        trstart = trstart_.Arguments[0];
                    }

                    var onitem = new Block();
                    onitem.Add(trstart.writeOut());
                    Block cellsout = BrailBuildingHelper.WriteOutCells(cells);
                    onitem.add(cellsout);
                    onitem.Add("</tr>".toLiteral().writeOut());

                    macro.findMacroContainer("foreach")["onitem"] = onitem;
                }
                else{
                    DefaultPrefix = "<tr><td>";
                    DefaultSuffix = "</td></tr>";
                }

                return base.Expand(macro);
            }
            }

        #endregion
    }
}