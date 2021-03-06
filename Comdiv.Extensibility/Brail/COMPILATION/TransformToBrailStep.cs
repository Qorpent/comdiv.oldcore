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

using System.IO;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

namespace Comdiv.Extensibility.Brail{
    //This class is responsible for taking a view script and transforming it to a legible
    //code. It does so by doing the following transformations:
    // * Add imports to Brail and Castle.MonoRail.Framework
    // * Create an overriding method call Run and trasnfer all the global code in the script ot it and then
    //   empty all the code in the global scope
    // * Create a class that inherit from BrailBase with the same name as the file 
    // * Add the Run method to this class
    // * Add any higher level elements (classes, methods, etc) to the newly created class
    //   and then remove them from the global scope
    // * Create a constructor that delegate to BrailBase constructor
    public class TransformToBrailStep : AbstractCompilerStep{
        private readonly ViewEngineOptions options;

        public TransformToBrailStep(ViewEngineOptions options){
            this.options = options;
        }

        public override void Run(){
            foreach (Module module in CompileUnit.Modules){
                foreach (string name in options.NamespacesToImport){
                    module.Imports.Add(new Import(module.LexicalInfo, name));
                }

                var macro = new ClassDefinition{Name = GetViewTypeName(module.FullName)};
                macro.BaseTypes.Add(new SimpleTypeReference(options.BaseType));

                AddConstructor(macro);
                ScriptDirectoryProperty(macro, module);
                AddRunMethod(macro, module);

                foreach (TypeMember member in module.Members){
                    macro.Members.Add(member);
                }

                module.Members.Clear();
                module.Members.Add(macro);
            }
        }

        public static string GetViewTypeName(string name){
            return "BrailView_" + name;
        }

        // get the directory name where this script reside and create a property
        // that return this value.
        // this is used to calculate relative paths when loading subviews.
        private void ScriptDirectoryProperty(ClassDefinition macro, Module module){
            var p = new Property("ScriptDirectory"){
                                                       Modifiers = TypeMemberModifiers.Override,
                                                       Getter = new Method("getScriptDirectory")
                                                   };
            p.Getter.Body.Add(
                new ReturnStatement(
                    new StringLiteralExpression(
                        Path.GetDirectoryName(module.LexicalInfo.FileName))));

            macro.Members.Add(p);
        }

        // create the Run method override for this class
        // this is where all the global code from the script goes
        private void AddRunMethod(ClassDefinition macro, Module module){
            var method = new Method("Run"){Modifiers = TypeMemberModifiers.Override, Body = module.Globals};
            module.Globals = new Block();
            macro.Members.Add(method);
        }

        // create a constructor that delegate to the base class
        private void AddConstructor(ClassDefinition macro){
            var ctor = new Constructor(macro.LexicalInfo);

            ctor.Parameters.Add(
                new ParameterDeclaration("viewEngine",
                                         new SimpleTypeReference(options.ViewEngineType)));
            // TODO: Update Reference
            var mie = new MethodInvocationExpression(new SuperLiteralExpression());
            mie.Arguments.Add(AstUtil.CreateReferenceExpression("viewEngine"));
            ctor.Body.Add(mie);

            macro.Members.Add(ctor);
        }
    }
}