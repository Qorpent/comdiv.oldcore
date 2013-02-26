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
using System.Linq;
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
        #region Nested type: BmlelementMacro

        /// <summary>
        /// nested macro to writeout xhtml content
        /// </summary>
        public class 
            BmlelementMacro : LexicalInfoPreservingMacro{
            protected override Statement ExpandImpl(MacroStatement macro){
                string tagname = macro.Arguments[0].ToCodeString();

                
                bool useend = macro.Arguments.Contains(x=>x.ToCodeString()=="___end");
                bool usestart = macro.Arguments.Contains(x => x.ToCodeString() == "___start");
                bool wastagmodifiers = useend||usestart;
        
                if(!wastagmodifiers){
                    useend = true;
                    usestart = true;
                }

                IEnumerable<Expression> argsource = macro.Arguments.Skip(1).Where(x=>!(x.ToCodeString().StartsWith("___")));
                StatementCollection statements = macro.Body.Statements;
                
                
                Statement result = tryResolveTemplate(macro, tagname, argsource);
                if (null == result){

                    result = expandBmlElement(tagname, argsource, statements, usestart,useend);
                }
                return result;
            
            }

            private Statement tryResolveTemplate(MacroStatement macro, string tagname, IEnumerable<Expression> argsource){
                MacroStatement bml = macro.findMacroContainer("bml");
                IDictionary<string, MacroStatement> templates = null;
                if (bml.ContainsAnnotation("templates")){
                    templates = bml.get<IDictionary<string, MacroStatement>>("templates");
                }
                if (templates != null && templates.ContainsKey(tagname)){
                    MacroStatement targetmacro = templates[tagname].CloneNode();
                    bindParameters(macro,targetmacro, argsource);
                    return targetmacro.Body;
                }
                return null;
            }

            private void bindParameters(MacroStatement macro, MacroStatement targetmacro, IEnumerable<Expression> expressions){
                List<Expression> srcarglist = expressions.ToList();
                List<Expression> substs = srcarglist.Where(x => !(x is BinaryExpression)).ToList();
                IDictionary<string,Expression> namedsubsts = new Dictionary<string, Expression>();
                foreach (var expr in expressions.OfType<BinaryExpression>()){
                    namedsubsts[expr.Left.fromLiteral()] = expr.Right;
                }
                var visitor = new TemplateBindVisitor(substs,namedsubsts,macro.Body);
                visitor.Visit(targetmacro);
                //foreach (BinaryExpression expression in srcarglist.OfType<BinaryExpression>()){
                //    BinaryExpression existed =
                //        targetmacro.Arguments.OfType<BinaryExpression>().FirstOrDefault(
                //            x => x.Left.ToCodeString() == expression.Left.ToCodeString());
                //    if (null == existed){
                //        targetmacro.Arguments.Add(expression);
                //    }
                //    else{
                //        existed.Right = expression.Right;
                //    }
                //}
            }

            private Statement expandBmlElement(string tagname, IEnumerable<Expression> argsource,
                                               StatementCollection statements, bool usestart, bool useend){
                var innerblock = new Block();

                if (usestart){
                    buildStartTag(tagname, argsource, innerblock);
                }

                foreach (Statement statement in statements)
                {
                    
                    innerblock.add(statement);

                }
                if (useend){
                    if (tagname != "bmlempty"){
                        innerblock.add(BrailBuildingHelper.WriteOut("</" + tagname + ">"));
                    }
                }
                return innerblock;
            }

            private void buildStartTag(string tagname, IEnumerable<Expression> argsource, Block innerblock) {
                IDictionary<string, string> defaults = null;
                if (DefaultAttributes.ContainsKey(tagname)){
                    defaults = DefaultAttributes[tagname];
                }
                ExpressionInterpolationExpression args = BrailBuildingHelper.getAttributes(argsource, defaults,
                                                                                           "class",
                                                                                           "id");

                if (tagname != "bmlempty"){
                    var tag = new ExpressionInterpolationExpression();
                    tag
                        .append("<" + tagname)
                        .append(args)
                        .append(">");
                    innerblock.add(BrailBuildingHelper.WriteOut(tag));
                }
            }
        }

        #endregion
    }
}