//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Comdiv.Extensions;
using Module = Boo.Lang.Compiler.Ast.Module;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    /// <summary>
    /// Переводит указанный модуль BOO в защищенный режим, 
    /// используется при построении компилятора расширений
    /// </summary>
    public class ExtensionsPreprocessor {
        private IEnumerable<Tuple<Assembly,string >> getmacronamespaces(CompilerContext context) {
            if (context["macronamespaces"] == null)
            {

                var macronamespaces = new List<Tuple<Assembly, string>>();
                context["macronamespaces"] = macronamespaces;
                foreach (ICompileUnit reference in context.References) {
                    try {
                        var a = reference.getPropertySafe<Assembly>("Assembly");
                        // импортирует автоматически макросы только из библиотек Comdiv - иначе жестокие тормоза и смысла главное никакого
                        if (a != null) {
                            var n = a.GetName().Name;
                            //HACK: due to performance issues uses only comdiv based libriries
                            if (!n.StartsWith("Comdiv.")) continue;
                            if (n.EndsWith(".Test")||n.EndsWith(".Tests")) continue;

         
                            var attrs = a.GetCustomAttributes(typeof(AssemblyBooMacroNamespaceAttribute), false);
                            if (attrs.Length != 0)
                            {
                                foreach (
                                    string ns in
                                        attrs.Cast<AssemblyBooMacroNamespaceAttribute>().Select(x => x.Namespace))
                                {
                                    macronamespaces.Add(Tuple.Create(a, ns));

                                }
                            }
                            
                        }
                    }
                    catch (Exception ex) {
                        context.TraceInfo("ошибка импорта пространства имен " + ex.Message);
                    }
                }
            }
            return (IEnumerable<Tuple<Assembly, string>>) context["macronamespaces"];
        }
        public Module ConvertModule(Module module, CompilerContext context) {
            var dir = Path.GetDirectoryName(module.LexicalInfo.FileName);
            
            var clsname = Path.GetFileNameWithoutExtension(module.LexicalInfo.FileName).Replace("-", "_").Replace(".",
                                                                                                                  "__");
            var prefix = "L0_";
            if(dir.like(@"(?ix)[\\/]sys[\\/]?(extensions[\\/]?)$")) {
                prefix = "L1_";
            }
            else if(dir.like(@"(?ix)[\\/]mod[\\/]?(extensions[\\/]?)$")) {
                prefix = "L2_";
            }
            else if(dir.like(@"(?ix)[\\/]usr[\\/]?(extensions[\\/]?)$")) {
                prefix = "L3_";
            }
            prefix += Path.GetFileName(dir) + "_";
            clsname = prefix + clsname;
            //clsname - имя класса, который инкапсулирует в себе тело модуля и является настройщиком реестра
            var methodbody = (Block) module.Globals.Clone();
            //method body - участок модуля с выполняемым кодом
            module.Globals = new Block();
            foreach (var n in getmacronamespaces(context))
            {
                module.Imports.Add(new Import(n.Item2, new ReferenceExpression(n.Item1.GetName().Name), null));
            }
            //зачищаем глобальную область модуля
            module.Imports.Add(new Import("Comdiv.Extensibility.IRegistryLoader", new ReferenceExpression("Comdiv.Core"),
                                          new ReferenceExpression("_IRL_")));
            module.Imports.Add(new Import("System.Collections.Generic",
                                          new ReferenceExpression("System"), null));
            //гарантируем присутствие интерфейса IRegistryLoader в импорте и IDictionary<string,string>
            var newclass = new ClassDefinition();
            newclass.Name = clsname;
            newclass.BaseTypes.Add(new SimpleTypeReference("_IRL_"));
            //класс реализует интерфейс IRegistryLoader
            var method = new Method("Load");
            var dictref = new GenericTypeReference();
            dictref.Name = "System.Collections.Generic.IDictionary";
            dictref.GenericArguments.Add(new SimpleTypeReference("string"));
            dictref.GenericArguments.Add(new SimpleTypeReference("object"));
            method.Parameters.Add(new ParameterDeclaration("registry", dictref));
            method.Body = methodbody;
            //формируем соотвествующий метод

            //теперь отправляем все глобальные функции в наш класс
            foreach (var function in module.Members.OfType<Method>().ToArray()) {
                newclass.Members.Add((Method) function.Clone());
                module.Members.Remove(function);
            }


            newclass.Members.Add(method);


            //добавляем его в класс
            module.Members.Add(newclass);
            //отправляем готовый класс обратно в модуль

            module.Annotate("regmethod", method.Body);

            return module;
        }
    }
}