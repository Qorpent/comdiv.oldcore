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
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Parser;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ExtensionsPreprocessorCompilerStep : AbstractCompilerStep {
        public override void Run() {
            foreach (var module in Context.CompileUnit.Modules) {
                new ExtensionsPreprocessor().ConvertModule(module, this.Context);
            }
        }

        public static CompilerPipeline Extend(CompilerPipeline pipeline) {
            if(pipeline.Find(typeof(ExtensionsPreprocessorCompilerStep))==-1) {
#if !LIB2
                if (pipeline.Find(typeof (BooParsingStep)) != -1) {
                    pipeline.InsertAfter(typeof (BooParsingStep), new ExtensionsPreprocessorCompilerStep());
                }
#else
                if (pipeline.Find(typeof(Parsing)) != -1)
                {
                    pipeline.InsertAfter(typeof(Parsing), new ExtensionsPreprocessorCompilerStep());
                }
#endif
            }
            return pipeline;
        }
    }
}