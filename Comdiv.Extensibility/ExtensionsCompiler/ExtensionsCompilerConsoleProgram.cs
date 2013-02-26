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
using System.Reflection;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ExtensionsCompilerConsoleProgram {
        public ExtensionsCompilerConsoleProgram() {
            WriteLog = s => { };
        }
        public Assembly Execute(string[] args  = null) {
            return Execute(new ExtensionsCompilerConsoleProgramArgsConverter().Get(args ?? new string[]{}));
        }

        public Assembly Execute(ExtensionsCompilerConsoleProgramArgs args) {
            WriteLog("start to process with arguments: " + args);
            var provider = new ExtensionsFileSystemProvider(args.Root, args.Web, args.DllName){WriteLog=s => WriteLog("\tfs:: "+s)};
            if (provider.IsNeedRecompile()) {
                WriteLog("need to recompile, start compiler...");
                new ExtensionsCompilerExecutor(s => WriteLog("\texec:: "+s)).Compile(provider);
                WriteLog("compilation finished");
            }else {
                WriteLog("no recompilation needed");
            }
            WriteLog("start load assembly");
            var result = provider.LoadAssembly();
            WriteLog("assembly loaded");
            return result;
        }

        public Action<string> WriteLog { get; set; }
    }
}