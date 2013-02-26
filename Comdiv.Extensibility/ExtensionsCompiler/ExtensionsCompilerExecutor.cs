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
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.MAS;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ExtensionsCompilerExecutor {
        
        public ExtensionsCompilerExecutor():this(null) {
            Pipeline = new CompileToFile();
        }
        public ExtensionsCompilerExecutor(Action<string> writeLog ) {
            this.WriteLog = writeLog ?? new Action<string>(s => { });
        }

        protected Action<string> WriteLog { get; set; }

        public CompilerContext LastCompiledContext { get; private set; }
        public CompilerPipeline Pipeline { get; set; }
        public Assembly Compile(ExtensionsFileSystemProvider provider) {
            var compiler = new BooCompiler();
            var pipeline = Pipeline ??( Pipeline = new CompileToFile());
            ExtensionsPreprocessorCompilerStep.Extend(pipeline);
            compiler.Parameters.Pipeline = pipeline;
            var files = provider.GetFileNames();
            if(files.Length==0) {
                WriteLog("no input files provided");
            }
            foreach (var fileName in files) {
                WriteLog("input added: " + fileName);
                compiler.Parameters.Input.Add(new FileInput(fileName));
            }
            compiler.Parameters.References.Add(typeof (IRegistryLoader).Assembly); //need to use Comdiv.Core
            compiler.Parameters.References.Add(typeof (IDictionary<string, object>).Assembly); //need to use System
            WriteLog("compiler created");
            //loading other dlls:
            foreach (var referencedAssembly in provider.GetReferencedAssemblies()) {
                WriteLog("add assembly " + referencedAssembly.GetName().Name);
                compiler.Parameters.References.Add(referencedAssembly);
            }
            compiler.Parameters.OutputAssembly = provider.GetLibraryPath();
            WriteLog("output is setted : " + provider.GetLibraryPath());
            WriteLog("start compiler");
            var result = compiler.Run();
            LastCompiledContext = result;
            if (result.Errors.Count != 0) {
                WriteLog("error occured!");
                WriteLog(result.Errors.ToString());
                ConsoleLogHost.Current.logerror(result.Errors.ToString());
                throw new CompilerErrorException(result);
            }
            //if (result.Warnings.Count != 0)
            //{
            //    WriteLog("warrnings!");
            //    WriteLog(result.Warnings.ToString());
            //}
            WriteLog("compilation successfull");
            if(Pipeline is CompileToMemory) {
                return result.GeneratedAssembly;
            }else {
                return provider.LoadAssembly();
            }
        }
    }
}