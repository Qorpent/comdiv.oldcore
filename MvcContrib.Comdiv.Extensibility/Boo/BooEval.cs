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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler;
using Boo.Lang.Interpreter;
using Boo.Lang.Parser;

namespace MvcContrib.Extensibility{
    public class BooEval{
        private static BooEval _default;
        private static BooEval _defaultWSA;

        private readonly ICompilerStep parser;
        private InteractiveInterpreter interpreter;
        public string RootDirectory = "/";
        public bool ThrowExceptions = true;

        public BooEval() {}


        public BooEval(ICompilerStep parser){
            this.parser = parser;
        }

        public static BooEval Default{
            get { return _default ?? (_default = new BooEval()); }
        }

        public static BooEval DefaultWSA{
            get { return _defaultWSA ?? (_defaultWSA = new BooEval(new WSABooParsingStep())); }
        }


        public InteractiveInterpreter Interpreter{
            get{
                if (null == interpreter){
                    interpreter = null == parser ? new InteractiveInterpreter() : new InteractiveInterpreter(parser);
                    interpreter.Ducky = true;
                    
                }
                return interpreter;
            }
        }

        public static BooEval BuildWSA(){
            //return new BooEval(new WSABooParsingStep());
            return new BooEval();
        }

        public object Evaluate(string expression){
            return Evaluate(expression, true);
        }

        public object Evaluate(string expression, IDictionary<string, object> globals){
            return Evaluate(expression, globals, true);
        }

        public object Evaluate(string expression, bool returnValue){
            return Evaluate(expression, null, returnValue);
        }

        public object Evaluate(string expression, IDictionary<string, object> globals, bool returnValue){
            return Evaluate(string.Empty, expression, globals, returnValue);
        }

        public object EvaluateFile(string path){
            return EvaluateFile(path, null);
        }

        public object EvaluateFile(string path, IDictionary<string, object> globals){
            return Evaluate(path, string.Empty, globals, true);
        }

        public object Evaluate(string path, string expression, IDictionary<string, object> globals){
            return Evaluate(path, expression, globals, true);
        }

        public object Evaluate(string path, string expression, IDictionary<string, object> globals, bool returnValue){
            Interpreter.RememberLastValue = returnValue;
            
            setupGlobals(globals);
            //loads default macroses
            try {
                catchExceptions("", evalExpression(@"import MvcContrib.Comdiv
import MvcContrib.Comdiv.Extensibility
import MvcContrib.Comdiv.Extensibility.BooLang
import MvcContrib.Comdiv.Framework"));
            } catch (Exception ex) {

            }
            catchExceptions(path, evalFile(path));
            catchExceptions("", evalExpression(expression));
            return interpreter.LastValue;
        }


        private void catchExceptions(string path, CompilerContext context){
            if (!ThrowExceptions){
                return;
            }
            if (null == context){
                return;
            }
            if (context.Errors.Count == 0){
                return;
            }
            var inner = new BooErrorException(context.Errors);
            throw new Exception("error in " + path, inner);
        }

        private CompilerContext evalExpression(string expression){
            if (!string.IsNullOrEmpty(expression)){
                return Interpreter.Eval(expression);
            }
            return null;
        }

        private CompilerContext evalFile(string path){
            if (!string.IsNullOrEmpty(path)){
                string script = GetScript(path);

                return interpreter.Eval(script);
            }
            return null;
        }

        public static string LoadScript(string path){
            return new BooEval{RootDirectory = Environment.CurrentDirectory}.GetScript(path);
        }

        protected string GetScript(string path){
            return GetScript(RootDirectory, path);
        }

        protected string GetScript(string rootPath, string path){
            string realPath = resolvePath(rootPath, path);

            string script = string.Empty;
            string cachedBoo = realPath + ".cached";

            if (File.Exists(cachedBoo) && !HasNewerBooFiles(cachedBoo)){
                script = File.ReadAllText(cachedBoo);
            }
            else{
                script = File.ReadAllText(realPath);
                script = preprocessScript(realPath, script);
                File.WriteAllText(cachedBoo, script);
            }
            return script;
        }

        private void setupGlobals(IDictionary<string, object> globals){
            if (null != globals){
                foreach (var c in globals){
                    Interpreter.SetValue(c.Key, c.Value);
                }
            }
        }

        private string resolvePath(string root, string path){
            if (!Path.IsPathRooted(path)){
                path = Path.Combine(root, path);
            }
            if (string.IsNullOrEmpty(Path.GetExtension(path))){
                path += ".boo";
            }
            return path;
        }


        public bool HasNewerBooFiles(string boo){
            var info = new FileInfo(boo);
            DateTime checkDate = info.LastWriteTime;
            return HasNewerBooFiles(info.Directory, checkDate);
        }

        public bool HasNewerBooFiles(DirectoryInfo dir, DateTime checkDate){
            foreach (FileInfo file in dir.GetFiles("*.boo", SearchOption.AllDirectories)){
                if (file.LastWriteTime > checkDate){
                    return true;
                }
            }
            return false;
        }


        protected virtual string preprocessScript(string path, string script){
            string result = script;
            result = resolveIncludes(path, script);
            return result;
        }

        private string resolveIncludes(string path, string srcscript){
            string dir = Path.GetDirectoryName(path);
            MatchEvaluator matcher =
                delegate(Match match) { return GetScript(Path.GetDirectoryName(path), match.Groups["name"].Value); };
            return Regex.Replace(srcscript, @"\#include\s+(?<name>\S+)", matcher);
        }

        public void EvalDirectory(){
            EvalDirectory(string.Empty);
        }

        public void EvalDirectory(string directory){
            if (directory == null){
                throw new ArgumentNullException("directory");
            }
            string dir = directory; //.ResolvePathExtensions();
            if (!Path.IsPathRooted(dir)){
                dir = Path.Combine(RootDirectory, directory);
            }
            if (!Directory.Exists(dir)){
                return;
            }
            foreach (string file in Directory.GetFiles(dir, "*.boo")){
                if (".boo" == Path.GetExtension(file)){
                    EvaluateFile(file);
                }
            }
        }

        #region Nested type: BooErrorException

        public class BooErrorException : Exception{
            public readonly CompilerErrorCollection Errors;


            public BooErrorException(CompilerErrorCollection errors) : base(errors.ToString()){
                Errors = errors;
            }
        }

        #endregion
    }
}