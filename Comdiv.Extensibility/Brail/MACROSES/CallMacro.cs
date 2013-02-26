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


using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility.Brail{
    /// <summary>
    /// Macro for quick RenderAction call
    /// render action[,controller][,hash of parameters]
    /// =>
    /// Html.RenderAction("action","controller"|null,RouteValueDictionary|null)
    /// </summary>
    public class CallMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            if (macro.Arguments.Count == 0)
            {
                Context.Errors.Add(new CompilerError(macro.LexicalInfo,
                                                     "call macro requires at least one reference or string attribute for action name"));
            }
            var basis = new ReferenceExpression("Html");
            var method = new MemberReferenceExpression(basis, "RenderAction");
            var call = new MethodInvocationExpression(method);
            int i = 0;
            var result = new Block();
            foreach (Expression argument in macro.Arguments){
                i++;
                Expression exp = argument;
                if (!(exp is HashLiteralExpression)){
//action and contrller parameters
                    if (!(exp is NullLiteralExpression)){
                        exp = new StringLiteralExpression(argument.ToCodeString());
                    }
                    call.Arguments.Add(exp);
                }
                else{
                    string name = "__rd";
                    result.Add(
                        new DeclarationStatement(
                            new Declaration(name, null),
                            new MethodInvocationExpression(AstUtil.CreateReferenceExpression("RouteValueDictionary"))
                            )
                        );
                    var dict = argument as HashLiteralExpression;
                    foreach (ExpressionPair item in dict.Items){
                        result.Add(
                            new MethodInvocationExpression(
                                AstUtil.CreateReferenceExpression(name + ".Add"),
                                item.First,
                                item.Second
                                )
                            );
                    }
                    if (i == 2){
                        call.Arguments.Add(new NullLiteralExpression());
                    }
                    call.Arguments.Add(AstUtil.CreateReferenceExpression(name));
                }
            }
            result.Add(call);
            return result;
        }
    }
}