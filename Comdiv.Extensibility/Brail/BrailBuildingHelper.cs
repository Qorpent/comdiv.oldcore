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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public static class BrailBuildingHelper{
        public static Expression WriteOut(object param){
            object _arg = param;
            if (!(param is Expression)){
                _arg = param.toLiteral();
            }
            return new MethodInvocationExpression(
                AstUtil.CreateReferenceExpression(
                    "__write"),
                (Expression) _arg);
        }

        public static Expression writeOut(this object obj){
            return WriteOut(obj);
        }

        public static Block WriteOutCells(IEnumerable<CellDefinition> cells){
            return WriteOutCells(cells, false);
        }

        public static Block WriteOutCells(IEnumerable<CellDefinition> cells, bool header){
            string tagname = header ? "th" : "td";
            var onitem = new Block();
            foreach (CellDefinition cell in cells){
                var opentag = new ExpressionInterpolationExpression();
                ExpressionInterpolationExpression attrs = getAttributes(cell.Attributes);
                opentag.append("<" + tagname).append(attrs).append(">");
                onitem.add(opentag.writeOut());
                onitem.add(cell.Value.brailOutResolve());

                onitem.Add(("</" + tagname + ">").toLiteral().writeOut());
            }
            return onitem;
        }

        public static Statement brailOutResolve(this Node node){
            if (null == node) return new ExpressionStatement(new StringLiteralExpression("").writeOut());
            var ex = node as Expression;
            var bl = node as Block;
            var st = node as Statement;
            if (ex != null){
                if (ex is LiteralExpression){
                    return new ExpressionStatement(ex.writeOut());
                }
                if (ex.ToCodeString().ToLower().StartsWith("out")){
                    return new ExpressionStatement(ex);
                }
                if (ex is ReferenceExpression || ex is MethodInvocationExpression || ex is BinaryExpression ||
                    ex is ExpressionInterpolationExpression){
                    return new ExpressionStatement(ex.writeOut());
                }

                else{
                    return new ExpressionStatement(ex);
                }
            }
            else if (bl != null){
                var result = new Block();
                foreach (Statement statement in bl.Statements){
                    result.Add(statement.brailOutResolve());
                }
                return result;
            }
            else if (st != null){
                if (st is ExpressionStatement){
                    return ((ExpressionStatement) st).Expression.brailOutResolve();
                }
                else{
                    return st;
                }
            }
            else{
                return new Block();
            }
        }

        public static Block add(this Block block, Node node){
           
            if (node is Block){
                block.Add((Block) node);
            }
            else if (node is Statement){
                block.Add((Statement) node);
            }
            else if (node is Expression){
                block.Add((Expression) node);
            }
            return block;
        }

        public static ExpressionInterpolationExpression getAttributes(IEnumerable<Expression> expressions,
                                                                      params string[] mainattributes){
            return getAttributes(expressions, new string[]{}, mainattributes);
        }

        public static ExpressionInterpolationExpression getAttributes(IEnumerable<Expression> expressions,
                                                                      IEnumerable<string> skips,
                                                                      params string[] mainattributes){
            return getAttributes(expressions, skips, null, mainattributes);
        }

        public static ExpressionInterpolationExpression getAttributes(IEnumerable<Expression> expressions,
                                                                      IDictionary<string, string> defaults,
                                                                      params string[] mainattributes){
            return getAttributes(expressions, new string[]{}, defaults, mainattributes);
        }

        public static ExpressionInterpolationExpression getAttributes(IEnumerable<Expression> expressions,
                                                                      IEnumerable<string> skips,
                                                                      IDictionary<string, string> defaults,
                                                                      params string[] mainattributes){
            if (mainattributes == null || mainattributes.Length == 0){
                mainattributes = new[]{"class", "id"};
            }
            var dict = new Dictionary<Expression, Expression>();


            int idx = 0;
            foreach (Expression argument in expressions){
                if (argument is HashLiteralExpression){
                    foreach (ExpressionPair pair in ((HashLiteralExpression) argument).Items){
                        dict[convertToAttribute(pair.First)] = convertToAttribute(pair.Second);
                    }
                    //it MUST be last statement
                    break;
                }
                if (argument is BinaryExpression){
                    var _binexp = (BinaryExpression) argument;
                    if (skips.Contains(_binexp.Left.ToCodeString())){
                        continue;
                    }
                    
                    if (_binexp.Right is ListLiteralExpression){
                        continue;
                    }
                    if (_binexp.Right is HashLiteralExpression){
                        continue;
                    }
                    var name = convertToAttribute(_binexp.Left);
                    var value = convertToAttribute(_binexp.Right);

                    //спецобработка для атрибута checked - заколебался с ним возиться в видах
                    bool proceed = true;
                    if(name.fromLiteral()=="checked"){
                        if(value.fromLiteral().ToLower()=="true"){
                            value = "checked".toLiteral();
                        }else{
                            proceed = false;
                        }
                    }
                    if(proceed){
                        dict[name] = value;
                    }
                }
                else{
                    Expression exp = convertToAttribute(argument);
                    if (idx <= 2){
                        dict[new StringLiteralExpression(mainattributes[idx])] = exp;
                    }
                    else{
                        throw new Exception(
                            "cannot understand target of attribute, only three anonymous supported for class,id,name");
                    }
                }
                idx++;
            }
            if (null != defaults){
                foreach (var def in defaults){
                    bool proceed = true;
                    foreach (var expression in dict){
                        if (expression.Key is StringLiteralExpression){
                            if (((StringLiteralExpression) expression.Key).Value == def.Key){
                                proceed = false;
                                break;
                            }
                        }
                    }
                    if (proceed){
                        dict[def.Key.toLiteral()] = def.Value.toLiteral();
                    }
                }
            }

            var result = new ExpressionInterpolationExpression();
            foreach (var node in dict){
                result.Expressions.Add(new StringLiteralExpression(" "));
                result.Expressions.Add(node.Key);
                result.Expressions.Add(new StringLiteralExpression("='"));
                if (node.Value is StringLiteralExpression){
                    string escaped = WebUtility.HtmlEncode(((StringLiteralExpression) node.Value).Value).Replace("'",
                                                                                                                  "&apos;");
                    result.Expressions.Add(escaped.toLiteral());
                }
                else if (node.Value is IntegerLiteralExpression){
                    result.Expressions.Add(node.Value.ToCodeString().toLiteral());
                }
                else{
                    var escape = new MethodInvocationExpression(new ReferenceExpression("_escape"), node.Value);
                    result.Expressions.Add(escape);
                }
                result.Expressions.Add(new StringLiteralExpression("'"));
            }


            return result;
        }

        public static Expression convertToAttribute(Expression expression){
            if (expression is MemberReferenceExpression){
                return expression;
            }
            if (expression is ReferenceExpression){
                if (expression.ToCodeString().StartsWith("@")){
                    return new ReferenceExpression(expression.ToCodeString().Substring(1));
                }
                else{
                    return new StringLiteralExpression(expression.ToCodeString());
                }
            }
            return expression;
        }
    }
}