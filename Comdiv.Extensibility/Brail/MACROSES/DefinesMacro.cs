// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
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
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public partial class DefinesMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            var result = new Block();
            foreach (Statement st in macro.Body.Statements){
                var decl = st as DeclarationStatement;
                var refer = st as ExpressionStatement;
                if(null==decl){
                    var ex = refer.Expression;
                    if (ex is MethodInvocationExpression){
                        decl =
                            new DeclarationStatement(
                                new Declaration(((MethodInvocationExpression) refer.Expression).Target.ToCodeString(),
                                                null), null);
                    }
                    if(ex is BinaryExpression){
                        var b = ex as BinaryExpression;
                        decl = new DeclarationStatement(
                            new Declaration(b.Left.ToCodeString(),null),b.Right
                            );
                    }
                }

                var bin = new BinaryExpression(BinaryOperatorType.Assign,
                                               new TryCastExpression(new ReferenceExpression(decl.Declaration.Name),
                                                                     decl.Declaration.Type),
                                               decl.Initializer);
                var def = new MacroStatement("definebrailproperty");
                def.Arguments.Add(bin);
                result.Add(def);
            }
            return result;
        }
    }
}