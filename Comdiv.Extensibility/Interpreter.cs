#define PRECOMPILE_EVAL
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Persistence;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;


namespace Comdiv.Extensibility{
    
    public class Interpreter : IDisposable{
        public static string booclsBase = @"namespace DYNAMICEVALUATOR
import Comdiv.Extensibility
import {0} from {1}
public class {2}(RootBasedExpressionEvaluatorBase[of {3}]):
    None as string = null
    
    public override def expression() as object :
        return {4}
";
        private ScriptEngine _i;
        private ScriptEngine i { get { return _i ?? (_i = PythonPool.Get()); } }
        public int CountEvals;

        BooCompiler _compiler;
        BooCompiler compiler{
            get{
                if(null==_compiler){
                    _compiler = new BooCompiler();
                    _compiler.Parameters.Pipeline = new global::Boo.Lang.Compiler.Pipelines.CompileToMemory();
                    _compiler.Parameters.AddAssembly(typeof(RootBasedExpressionEvaluatorBase).Assembly);
                    _compiler.Parameters.GenerateInMemory = true;
                }
                return _compiler;
            }
        }


        private ScriptScope currentScope;
        public List<string> Expressions = new List<string>();

        private ILog log = logger.get("python");

        #region IDisposable Members

        public void Dispose(){
            if (null != i){
                PythonPool.Release(i);
            }
        }

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public IEnumerable<object> Query(Type type, string query, params object[] advancedParameters)
        {
            return myapp.storage.GetDefault().Query(type,(object[])new object[]{ query}.Union (advancedParameters).ToArray()).Cast<object>();
        }

        public IEnumerable<object> Query(string typeRef, string query, params object[] advancedParameters)
        {
            return myapp.storage.GetDefault().Query((Type)Type.GetType(typeRef), (object[])new object[] { query }.Union(advancedParameters).ToArray()).Cast<object>();
        }


        #endregion

        public object Eval(IDictionary<string, object> initValues, string returnValueName, string Expression){
            log.debug(() => "start eval " + Expression);
            Expressions.Add(Expression);
            CountEvals++;
            currentScope = i.CreateScope();

            foreach (var pair in initValues){
                i.Runtime.LoadAssembly(pair.Value.GetType().Assembly);

                currentScope.SetVariable(pair.Key,pair.Value);

            }
            i.Runtime.LoadAssembly(typeof (DateTime).Assembly);
            i.Runtime.LoadAssembly(typeof (CultureInfo).Assembly);
            string ex = "from System import *\r\n" + ReadDefaultImports() + "\r\n" + Expression;

            i.CreateScriptSourceFromString(ex, SourceCodeKind.File).Execute(currentScope);

            object res = null;
            if (string.IsNullOrWhiteSpace(returnValueName)){
                res = i.Execute(returnValueName, currentScope);
            }
            log.debug(() => "finish with " + res);
            return res;
        }

        private string ReadDefaultImports(){
            var fs = myapp.files;
            if (null == fs){
                return "";
            }
            return fs.ReadConcat("extensions/site.py");
        }
        static IDictionary<string,IRootBasedExpressionEvaluator> evaluators = new Dictionary<string, IRootBasedExpressionEvaluator>();
        public object Eval(object root, string expression){
            log.debug(() => "start eval-root " + expression);
            
            if (expression.StartsWith("hql:")){
                string hql = expression.Substring(4);

                return Container.first<IHqlEvaluator>().Execute(hql);
            }
            //HACK: to rewind Query calls from DataHelper which is removed from system
            if(root!=this && expression.StartsWith("Query(")){
                return Eval(this, expression);
            }
#if PRECOMPILE_EVAL
            
            string cacheKey = checkEvaluator(expression, root);
            return evaluators[cacheKey].Eval(root);
#else

            var values = new Dictionary<string, object>();
            values["x"] = root;
            //HACK: чтобы отвязаться от хелперов...
            if (expression.StartsWith("$")){
                expression = "res = " + expression.Substring(1);
            }
            else if (expression.StartsWith("#")){
                expression = expression.Substring(1);
            }
            else{
                expression = "res = x" + (expression.noContent() ? string.Empty : "." + expression);
            }
            object result = Eval(values, "res", expression);
            log.debug(() => "finish with " + result);

            return result;
#endif
        }

        private string checkEvaluator(string expression, object root) {
            var roottype = root.GetType();
            if(roottype.Name.Contains("Proxy")){
                roottype = roottype.BaseType;
            }
            var cacheKey = roottype.FullName + "." + expression;
            
            
            if(!evaluators.ContainsKey(cacheKey)){
                var dllname = roottype.FullName.Replace(".", "_") + "__" + expression.toSystemName();
                if (expression.StartsWith("$"))
                {
                    expression = expression.Substring(1);
                }
                else
                {
                    expression = "target" + (string.IsNullOrWhiteSpace(expression) ? string.Empty : "." + expression);
                }
                var ns = roottype.Namespace;
                var ass = roottype.Assembly.GetName().Name;
                var newtn = dllname;
                var extn = roottype.FullName;
                var code = booclsBase._format(ns, ass, newtn, extn, expression);
                compiler.Parameters.OutputAssembly = dllname + ".dll";
                compiler.Parameters.Input.Clear();
                compiler.Parameters.Input.Add(new ReaderInput("formula", new StringReader(code)));
                var result = compiler.Run();
                if (result.Errors.Count != 0)
                {
                    throw new Exception("Ошибка в выражении ("+roottype+": " + expression + "):\r\n" + result.Errors);
                }
                var assembly = result.GeneratedAssembly;
                var type = assembly.GetType("DYNAMICEVALUATOR." + dllname);
                var obj = type.create<IRootBasedExpressionEvaluator>();
                evaluators[cacheKey] = obj;
            }
            return cacheKey;
        }

        ~Interpreter(){
            Dispose();
        }
    }
}

