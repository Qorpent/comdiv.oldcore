using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Comdiv.Extensions;


namespace Comdiv.Extensibility{
    public class BooScriptMachine : IScriptMachine{
        private readonly Dictionary<string, object> _defaultValues = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _registry = new Dictionary<string, object>();
        private BooEval _boo;

        private BooEval Boo{
            get{
                lock (this){
                    Reload();

                    return _boo;
                }
            }
        }

        public string[] RootDirs { get; set; }

        public Dictionary<string, object> Registry{
            get { return _registry; }
        }

        public Dictionary<string, object> DefaultValues{
            get { return _defaultValues; }
        }

        public Assembly[] References
        {
            get; set;
        }

        public object Eval(string expression, IDictionary<string, object> values){
            lock (this){
                return Boo.Evaluate(expression, values);
            }
        }

        public virtual T Get<T>(string name){
            lock (this) return Registry.get<T>(name);
        }

        public virtual IList<T> Get<T>(){
            lock (this) return Registry.Values.OfType<T>().ToList();
        }

        private volatile bool _inreloadMode;

        public virtual void Reload(){
            lock (this){
                if (_inreloadMode){
                    while (_inreloadMode){
                        Thread.Sleep(10);
                    }
                    return;
                }
                _inreloadMode = true;
                try{
                    if (null == _boo) _boo = new BooEval();

                    if (RootDirs.Length == 0) return;
                    _boo.Interpreter.Reset();
                    if (References != null){
                        foreach (var assembly in References){
                            _boo.Interpreter.References.Add(assembly);
                        }
                    }
                    Registry.Clear();
                    foreach (var rootDir in RootDirs){
                        if (!Directory.Exists(rootDir)) continue;


                        _boo.Interpreter.SetValue("registry", Registry);
                        foreach (var pair in DefaultValues)
                            _boo.Interpreter.SetValue(pair.Key, pair.Value);
                        _boo.EvalDirectory(rootDir + "/imports");
                        _boo.EvalDirectory(rootDir);
                    }

                }
                finally{
                    _inreloadMode = false;
                }
            }
        }
    }
}