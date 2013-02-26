// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
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

using System;
using System.IO;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using MvcContrib.Comdiv.ViewEngines.Brail;


namespace MvcContrib.Comdiv.Extensibility.TestSupport{
    /// <summary>
    /// Provides convient for macro expansion checking
    /// call checkMacro(code, result) to
    /// check results of macro expanding 
    /// not needs to import Namespaces directly and
    /// make checks on imports, both elimenated and
    /// work-around by checkMacro
    /// </summary>
    public abstract class BaseMacroTestFixture<MacroType> {
        private BooCompiler compiler;
        public bool DoNotThrowOnError { get; set; }
        protected CompilerContext compile(string code){
            try{
                DoNotThrowOnError = true;
                return checkMacro(code, null);
            }finally{
                DoNotThrowOnError = false;
            }
        }
        protected CompilerContext parse(string code)
        {
            try
            {
                DoNotThrowOnError = true;
                return checkMacro(code, null, new Parse());
            }
            finally
            {
                DoNotThrowOnError = false;
            }
        }
        protected CompilerContext checkMacro(string code, string expectedresult)
        {
            
            return checkMacro(code, expectedresult, new ExpandMacros());
        }

        protected CompilerContext checkMacro(string code, string expectedresult, CompilerPipeline pipeline)
        {
            return checkMacro(code, expectedresult, pipeline, null);
        }

        protected  CompilerContext checkMacro(string code, string expectedresult,CompilerPipeline pipeline, TextWriter writer){
            code = stripBrail(code);
            compiler = new BooCompiler();
            compiler.Parameters.LoadDefaultReferences();
            compiler.Parameters.Pipeline = preparePipeline(pipeline) ?? pipeline; ;
            
            compiler.Parameters.References.Add(typeof (MacroType).Assembly);
            compiler.Parameters.References.Add(typeof(BmlMacro).Assembly);
            compiler.Parameters.References.Add(typeof(OutputMacro).Assembly);
            if(null!=writer)compiler.Parameters.OutputWriter = writer;
            //provide namespace of Macro
            compiler.Parameters.Input.Add(new StringInput("test","import "+typeof(MacroType).Namespace+"\r\n"
                + "import " + typeof(BmlMacro).Namespace + "\r\nimport " + typeof(OutputMacro).Namespace + "\r\n"
                + code));
            
            
            //var sw = new StringWriter();
            //compiler.Parameters.OutputWriter = sw;
            CompilerContext result = compiler.Run();
            if(result.Errors.Count!=0){
                if(DoNotThrowOnError) return result;
                Console.WriteLine("wrong code: "+result.CompileUnit.ToCodeString());
                throw new Exception(result.Errors.ToString());
            }
            string str = result.CompileUnit.ToCodeString();
            
            //remove namespaces from result and make trimming
            
            var str_ = StringExtensions.simplifyBoo(str);

            if (null != expectedresult) {
                var expectedresult_ = StringExtensions.simplifyBoo(expectedresult);
                if (!str_.Equals(expectedresult_)){
                    Console.WriteLine(code);
                    Console.WriteLine("==>");
                    Console.WriteLine(str); //for more readability}
                    Console.WriteLine("simplified:");
                    Console.WriteLine(str_.Replace("~~","'"));
                    throw new Exception("compilation does not match, expected:\r\n" + expectedresult_);
                }
                
                    
            }
            return result;

        }

        protected  virtual CompilerPipeline preparePipeline(CompilerPipeline pipeline){
            return null;
        }


        private string stripBrail(string code){
            code = code.Replace("<%", "").Replace("%>", "");
            code = Regex.Replace(code, @"^\s*end[\r\n]+", "", RegexOptions.Multiline);
            return code;
        }
    }
}