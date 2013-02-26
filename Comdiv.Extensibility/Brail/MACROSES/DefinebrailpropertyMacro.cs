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
    //public partial class DefinesMacro
    //{
        public class DefinebrailpropertyMacro : LexicalInfoPreservingMacro
        {
            protected override Statement ExpandImpl(MacroStatement macro)
            {
                Module module = macro.findModule();

                string propname = "";
                TypeReference type = new SimpleTypeReference("System.Object");
                Expression initializer = null;

                if (macro.Arguments[0] is BinaryExpression)
                {
                    var bin = macro.Arguments[0] as BinaryExpression;
                    initializer = bin.Right;
                    if (bin.Left is TryCastExpression)
                    {
                        var tce = bin.Left as TryCastExpression;
                        propname = tce.Target.ToCodeString();
                        type = tce.Type ?? type;
                    }
                    //else
                    //{
                    //    propname = bin.Left.ToCodeString();
                    //}
                }
                //else
                //{
                //    propname = macro.Arguments[0].ToCodeString();
                //}

                var geter = new Method("get_" + propname);
                geter.ReturnType = type;
                geter.Body = new Block();
                if (initializer != null)
                {
                    geter.Body.add(
                        new IfStatement(
                            new BinaryExpression(
                                BinaryOperatorType.Equality,
                                new NullLiteralExpression(),
                                new MethodInvocationExpression(
                                    new ReferenceExpression("TryGetParameterNoIgnoreNull"),
                                    new StringLiteralExpression(propname))
                                ),
                            new Block().add(new MethodInvocationExpression(
                                                new ReferenceExpression("SetProperty"),
                                                new StringLiteralExpression(propname),
                                                initializer ?? new NullLiteralExpression()))
                            , null
                            )
                        );
                }


                var convertmethod = new GenericReferenceExpression();
                convertmethod.Target = new ReferenceExpression("_convert");
                convertmethod.GenericArguments.Add(type);
                var valuecall = new MethodInvocationExpression(
                    new ReferenceExpression("TryGetParameterNoIgnoreNull"),
                    new StringLiteralExpression(propname));
                var convertcall = new MethodInvocationExpression(convertmethod,valuecall);
                
                geter.Body.add(new ReturnStatement(new CastExpression(convertcall, type)));

                var seter = new Method("set_" + propname);
                seter.Body = new Block().add(new MethodInvocationExpression(
                                                 new ReferenceExpression("SetProperty"),
                                                 new StringLiteralExpression(propname),
                                                 new ReferenceExpression("value")));

                var prop = new Property(geter, seter, type);
                prop.Name = propname;
                //try
                //{
                    ((TypeDefinition)module.Members[0]).Members.Insert(0, prop);
                //}
                //catch
                //{
                //    module.Members.Insert(0, prop); //in test env
                //}
                return null;
            }
        }
    }
//}