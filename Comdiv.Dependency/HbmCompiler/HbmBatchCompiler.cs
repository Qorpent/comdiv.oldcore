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
using System.Reflection;
using Comdiv.Extensions;
using FluentNHibernate;

namespace Comdiv.HbmCompiler {
    public class HbmBatchCompiler {
        public void Execute(HbmCompilerConsoleProgramArgs arguments, Action<string> writelog = null) {
            writelog = writelog ?? (s => { });
            var types = GetTypes(arguments, writelog);
            var compiler = new HbmCompiler();
            foreach (var type in types) {
                var outdir = GetOutDir(type, arguments);
                compiler.Execute(type, outdir, writelog);
            }
        }

        private string GetOutDir(Type type, HbmCompilerConsoleProgramArgs arguments) {
            return Path.Combine(arguments.OutDir, type.FullName.Replace(".", "_"));
        }

        private IEnumerable<Type> GetTypes(HbmCompilerConsoleProgramArgs arguments, Action<string> writelog) {
            ResolveEventHandler r = (s, a) => Assembly.LoadFrom(Path.Combine(arguments.Root, a.Name + ".dll"));
            try {
                AppDomain.CurrentDomain.AssemblyResolve+=r;

                var assemblynames = arguments.Assemblies.split();
                foreach (var assemblyname in assemblynames) {
                    var path = Path.Combine(arguments.Root, assemblyname + ".dll");
                    var assembly = ReflectionExtensions.LoadAssemblyFromFile(path);
                    foreach (var type in assembly.GetTypes()) {
                        if (typeof (PersistenceModel).IsAssignableFrom(type)) {
                            yield return type;
                        }
                    }
                }
            }finally {
                AppDomain.CurrentDomain.AssemblyResolve-=r;
            }
        }
    }
}