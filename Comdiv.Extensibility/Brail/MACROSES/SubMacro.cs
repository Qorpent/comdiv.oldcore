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

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    /// <summary>
    /// Macro for quick call to OutputSubView
    /// NOTE: assume that "viewname" and viewname is SAME for SubMacro,
    /// if you need to do call to subview, described by variable,
    /// use OutputSubView
    /// subview "/viewname"||viewname, args
    /// =>
    /// OutputSubView("viewname", args)
    /// </summary>
    public class SubMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            if (macro.Arguments.Count == 0){
                Context.Errors.Add(new CompilerError(macro.LexicalInfo,
                                                     "sub macro requires at least one reference or string attribute for subview name"));
            }

            var call = new MethodInvocationExpression(AstUtil.CreateReferenceExpression("OutputSubView"));
            int i = 0;
            foreach (Expression argument in macro.Arguments){
                i++;
                Expression exp = argument;
                if (i == 1){
//action and contrller parameters
                    if (argument is ReferenceExpression && !(argument is MemberReferenceExpression)){
                        if (argument.ToCodeString().StartsWith("@") || argument.ToCodeString().Contains(".")){
                            exp = AstUtil.CreateReferenceExpression(argument.ToCodeString().Substring(1));
                        }
                        else{
                            exp = new StringLiteralExpression(argument.ToCodeString());
                        }
                    }
                }
                call.Arguments.Add(exp);
            }
            return new ExpressionStatement(call);
        }
    }
}