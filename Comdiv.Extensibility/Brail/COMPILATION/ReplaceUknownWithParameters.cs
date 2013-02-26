// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
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
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.TypeSystem;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail{
    ///<summary>
    /// Replace any uknown identifier with a call to GetParameter('unknown')
    /// this mean that unknonw identifier in scripts will only fail in run time if they
    /// were not defined by the controller.
    /// </summary>
    public class ReplaceUknownWithParameters : ProcessMethodBodiesWithDuckTyping{
        private IMethod getParam;
        private IMethod tryGetParam;
        
        public override void OnReferenceExpression(ReferenceExpression node){
            if (dowork) {
                IEntity entity = NameResolutionService.Resolve(node.Name);
                if (entity != null) {
                    
                    base.OnReferenceExpression(node);
                    Console.Write(".");
                    //     _context.TraceInfo(".");
                }
                else {
                    replacecount++;
                    MethodInvocationExpression mie = CodeBuilder.CreateMethodInvocation(
                        CodeBuilder.CreateSelfReference(_currentMethod.DeclaringType),
                        GetMethod(node.Name));
                    mie.Arguments.Add(GetNameLiteral(node.Name));
                    node.ParentNode.Replace(node, mie);
                    Console.Write("!(" + node.Name + ")");
                    //    _context.TraceInfo("!");
                }
            }else {
                base.OnReferenceExpression(node);
            }
        }

        private int cnt = 0;
        private int replacecount = 0;
        private bool dowork = false;
        public override void OnModule(Module module) {
            replacecount = 0;
            dowork = module["isduck"]==null|| module["isduck"].toBool();

                //NameResolutionService.Reset();
                Console.WriteLine("");
                Console.Write("[" + module.Members[0].Name.Replace("_0_","/") + "](" + (cnt++) + ")");
                base.OnModule(module);
                if(replacecount==0) {
                    
                    if (dowork) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine();
                        Console.WriteLine(module.Members[0].Name.Replace("_0_", "/") + " can be NODUCK");
                    }else {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine(module.Members[0].Name.Replace("_0_", "/") + " is NODUCK");
                    }
                   
                }else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine(module.Members[0].Name.Replace("_0_", "/") + " contains DUCKS");
                }

                Console.ResetColor();
            
        }

        protected override void InitializeMemberCache(){
            base.InitializeMemberCache();
            getParam = TypeSystemServices.Map(typeof (BrailBaseCommon).GetMethod("GetParameter"));
            tryGetParam = TypeSystemServices.Map(typeof (BrailBaseCommon).GetMethod("TryGetParameter"));
        }

        public IMethod GetMethod(string name){
            if (name[0] == '?'){
                return tryGetParam;
            }
            else{
                return getParam;
            }
        }

        public StringLiteralExpression GetNameLiteral(string name){
            if (name[0] == '?'){
                return CodeBuilder.CreateStringLiteral(name.Substring(1));
            }
            else{
                return CodeBuilder.CreateStringLiteral(name);
            }
        }
    }
}