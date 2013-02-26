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
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public abstract class ForeachSnippet<T> : LexicalInfoPreservingMacro where T : ForeachSnippet<T>{
        protected string[] SkipBinaryParametersOnGetAttributes = new string[]{};
        public object BeforeAll { get; set; }

        public object AfterAll { get; set; }

        public object BeforeEach { get; set; }

        public object AfterEach { get; set; }

        public object Between { get; set; }
        protected string DefaultPrefix { get; set; }
        protected string DefaultSuffix { get; set; }

        public T beforeall(object obj){
            BeforeAll = obj;
            return (T) this;
        }

        public T afterall(object obj){
            AfterAll = obj;
            return (T) this;
        }

        public T beforeeach(object obj){
            BeforeEach = obj;
            return (T) this;
        }

        public T aftereach(object obj){
            AfterEach = obj;
            return (T) this;
        }

        public T between(object obj){
            Between = obj;
            return (T) this;
        }

        protected override Statement ExpandImpl(MacroStatement macro){
            ExpressionInterpolationExpression attr = BrailBuildingHelper.getAttributes(macro.Arguments,
                                                                                       SkipBinaryParametersOnGetAttributes,
                                                                                       new string[]{});

            prepare(BeforeAll, "beforeall", attr, macro);
            prepare(AfterAll, "afterall", attr, macro);
            prepare(BeforeEach, "beforeeach", attr, macro);
            prepare(AfterEach, "aftereach", attr, macro);
            prepare(Between, "between", attr, macro);
            if (!string.IsNullOrEmpty(DefaultPrefix)){
                macro.findMacroContainer("foreach").set("_prefix", DefaultPrefix);
                macro.findMacroContainer("foreach").set("_suffix", DefaultSuffix);
            }
            return null;
        }

        protected virtual void prepare(object val, string name, Expression attr, MacroStatement macro){
            if (val != null){
                if (!macro.findMacroContainer("foreach").ContainsAnnotation(name)){
                    object realexpression = val;
                    if (realexpression is string){
                        if (((string) realexpression).Contains("#ATTR")){
                            var interpolation = (ExpressionInterpolationExpression) attr;
                            realexpression = realexpression.ToString().Replace("#ATTR>", "").toLiteral();
                            interpolation.Expressions.Insert(0, (Expression) realexpression);
                            interpolation.Expressions.Add(new StringLiteralExpression(">"));
                            realexpression = BrailBuildingHelper.WriteOut(interpolation);
                        }
                        else{
                            realexpression = realexpression.ToString().toLiteral();

                            realexpression = BrailBuildingHelper.WriteOut(realexpression);
                        }
                    }
                    if (name == "beforeall"){
                        realexpression = onbeforeall(realexpression, attr, macro) ?? realexpression;
                    }
                    if (name == "beforeeach"){
                        realexpression = onbeforeeach(realexpression, attr, macro) ?? realexpression;
                    }
                    if (name == "afterall"){
                        realexpression = onafterall(realexpression, attr, macro) ?? realexpression;
                    }
                    if (name == "aftereach"){
                        realexpression = onaftereach(realexpression, attr, macro) ?? realexpression;
                    }
                    if (name == "between"){
                        realexpression = onbetween(realexpression, attr, macro) ?? realexpression;
                    }
                    macro.findMacroContainer("foreach")[name] = realexpression;
                }
            }
        }

        protected virtual object onbeforeall(object realexpression, Expression expression, MacroStatement statement){
            return null;
        }

        protected virtual object onafterall(object realexpression, Expression expression, MacroStatement statement){
            return null;
        }

        protected virtual object onbeforeeach(object realexpression, Expression expression, MacroStatement statement){
            return null;
        }

        protected virtual object onaftereach(object realexpression, Expression expression, MacroStatement statement){
            return null;
        }

        protected virtual object onbetween(object realexpression, Expression expression, MacroStatement statement){
            return null;
        }

        protected object getAttributedTagFromMacroExtensionParameters(MacroStatement macro, string tagname){
            return getAttributedTagFromMacroExtensionParameters(macro, tagname, tagname, null);
        }

        protected object getAttributedTagFromMacroExtensionParameters(MacroStatement macro, string tagname,
                                                                      string[] defaultAttributes){
            return getAttributedTagFromMacroExtensionParameters(macro, tagname, tagname, defaultAttributes);
        }

        protected object getAttributedTagFromMacroExtensionParameters(MacroStatement macro, string parameterName,
                                                                      string tagname){
            return getAttributedTagFromMacroExtensionParameters(macro, parameterName, tagname, null);
        }


        protected object getAttributedTagFromMacroExtensionParameters(MacroStatement macro, string parameterName,
                                                                      string tagname, string[] defaultAttributes){
            object result = null;
            BinaryExpression option = getExtensionArgument(macro, parameterName);
            if (option != null){
                IEnumerable<Expression> expressions = expand_to_expressions(option.Right);
                ExpressionInterpolationExpression attributes = BrailBuildingHelper.getAttributes(expressions,
                                                                                                 defaultAttributes);
                attributes.Expressions.Insert(0, ("<" + tagname).toLiteral());
                attributes.Expressions.Add(">".toLiteral());
                result = BrailBuildingHelper.WriteOut(attributes);
            }
            return result;
        }

        protected BinaryExpression getExtensionArgument(MacroStatement macro, string proptofind){
            return macro.Arguments.OfType<BinaryExpression>().Where(x => x.Left.ToCodeString() == proptofind).
                FirstOrDefault();
        }

        protected IEnumerable<Expression> expand_to_expressions(Expression expression){
            if (expression is ListLiteralExpression){
                var list = (ListLiteralExpression) expression;
                foreach (Expression item in list.Items){
                    yield return item;
                }
            }
            
            else{
                yield return expression;
            }
        }
    }
}