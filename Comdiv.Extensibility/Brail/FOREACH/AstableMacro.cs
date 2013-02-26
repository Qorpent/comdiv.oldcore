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
        #region Nested type: AstableMacro

        public class AstableMacro : ForeachSnippet<AstableMacro>{
            private IList<CellDefinition> cells;
            private bool generateHeader;
            private IList<CellDefinition> heads;

            public AstableMacro(){
                DefaultPrefix = "<td>";
                DefaultSuffix = "</td>";
                SkipBinaryParametersOnGetAttributes = new[]{"row"};
                beforeall("<table#ATTR>")
                    .beforeeach("<tr>")
                    .aftereach("</tr>")
                    .afterall("</table>");
            }

            protected override object onbeforeeach(object realexpression, Expression expression,
                                                   MacroStatement statement){
                return getAttributedTagFromMacroExtensionParameters(statement, "row", "tr");
            }

            protected override object onbeforeall(object realexpression, Expression expression, MacroStatement statement){
                if (generateHeader){
                    var result = new Block();
                    var tag = new ExpressionInterpolationExpression();
                    tag.append("<table".toLiteral());
                    tag.append(BrailBuildingHelper.getAttributes(statement.Arguments));
                    tag.append(">".toLiteral());
                    tag.append("<thead><tr>".toLiteral());
                    result.add(tag.writeOut());
                    Block headout = BrailBuildingHelper.WriteOutCells(heads, true);
                    result.add(headout);
                    result.add("</tr></thead><tbody>".toLiteral().writeOut());
                    return result;
                }
                else{
                    return null;
                }
            }

            protected override object onafterall(object realexpression, Expression expression, MacroStatement statement){
                if (generateHeader){
                    return "</tbody></table>".toLiteral().writeOut();
                }
                else{
                    return null;
                }
            }

            private void assumeHeadsAndCellsCountMatch(){
                if (null != cells && null != heads){
                    if (heads.Count > cells.Count){
                        for (int i = 0; i < heads.Count - cells.Count; i++){
                            cells.Add(new CellDefinition
                                      {Value = new StringLiteralExpression(""), Attributes = new Expression[]{}});
                        }
                    }
                    if (heads.Count < cells.Count){
                        for (int i = 0; i < cells.Count - heads.Count; i++){
                            heads.Add(new CellDefinition
                                      {Value = new StringLiteralExpression(""), Attributes = new Expression[]{}});
                        }
                    }
                }
            }

            public override Statement Expand(MacroStatement macro){
                extractCellsAndHeaders(macro);
                macro.findMacroContainer("foreach")["istable"] = true;
                if (heads != null){
                    generateHeader = true;
                }
                if (cells != null){
                    macro.findMacroContainer("foreach")["onitem"] = BrailBuildingHelper.WriteOutCells(cells);
                }

                return base.Expand(macro);
            }

            private void extractCellsAndHeaders(MacroStatement macro){
                cells = macro.get<IList<CellDefinition>>("_cells");
                heads = macro.get<IList<CellDefinition>>("_heads");
                assumeHeadsAndCellsCountMatch();
            }
        }

        #endregion
    }
}