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
using System.IO;
using Comdiv.ConsoleUtils;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ExtensionsCompilerConsoleProgramArgsConverter {
        //TODO: потом заменить на новый универсальный парсер
        public ExtensionsCompilerConsoleProgramArgs Get(string[] args) {
            var result = args.toObject<ExtensionsCompilerConsoleProgramArgs>();
            if (!Path.IsPathRooted(result.Root)) {
                result.Root = Path.Combine(Environment.CurrentDirectory, result.Root);
            }
            return result;

            
        }
    }
}