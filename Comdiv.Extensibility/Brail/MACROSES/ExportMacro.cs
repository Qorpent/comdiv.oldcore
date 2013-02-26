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
    public class ExportMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            var call = new MethodInvocationExpression(
                new ReferenceExpression("__Export")
                );
            call.Arguments.Add(new StringLiteralExpression(macro.Arguments[0].ToCodeString()));
            if(macro.Arguments.Count==2){
                call.Arguments.Add(macro.Arguments[1]);
            }
            return new ExpressionStatement(call);
        }
    }
}