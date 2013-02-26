using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Comdiv.Application;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail
{
    public class ViewTypeFactory
    {
        public ViewTypeFactory() {
            
        }

        public void CompileAll() {
            Compiler.Resolver = this.Resolver;
            ApplicationAssembly = Compiler.CompileApplication(Options);
        }

        protected IViewCompiler Compiler { get; set; }

        public ViewTypeFactory(IViewSourceResolver resolver, ViewEngineOptions options, IViewCompiler compiler) {
            this.Resolver = resolver;
            this.Options = options;
            this.Compiler = compiler;
        }

        public ViewEngineOptions Options { get; set; }

        public IViewSourceResolver Resolver { get; set; }

        public T CreateSubView<T>(string parent,string subview, Type[] types=null,object[] parameters=null) {
            return CreateView<T>(Resolver.ResolveSubView(parent, subview), types, parameters);
        }

        public T CreateView<T>(string key, Type[] types=null, object[] parameters=null) {
            if (!key.StartsWith("/")) key = "/" + key;
            key = key.normalizePath().ToLower();
            var type = internalGetType(key);
            types = types ?? Type.EmptyTypes;
            parameters = parameters ?? new object[] {};
            return type.create<T>(types, parameters);
        }
        private bool? applicationAssemblyExists;
        private  Assembly _applicationassembly;
        object sync = new object();
        Assembly ApplicationAssembly {
            get {
                lock (sync) {
                    if (null != _applicationassembly) return _applicationassembly;
                    if (applicationAssemblyExists.HasValue && !applicationAssemblyExists.Value) return null;
                    var loaded =
                        AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "_application_views");
                    if(null!=loaded) {
                        _applicationassembly = loaded;
                        
                    }else {
                        var filename = myapp.files.Resolve("~/tmp/_application_views.dll", true);
                        if(filename.hasContent()) {
                            File.Copy(filename, filename + "2",true);
                            _applicationassembly = Assembly.LoadFrom(filename+2);
                        }
                    }
                    if(_applicationassembly==null) {
                        applicationAssemblyExists = false;
                    }
                    return _applicationassembly;

                }
            }
            set { _applicationassembly = value; }
        }


        protected virtual Type internalGetType(string key) {
            var info = Resolver.GetFullInfo(key);
            var existed = cache.get(key);
            Type result = null;
            if(null==existed||(info.LastModified-existed.Timestamp).TotalMilliseconds>1000) {
                lock(sync) {
                    if(existed==null && ApplicationAssembly!=null) {
                        result = ApplicationAssembly.GetType(key.Replace("/", "_0_"), false, true);
                        if(null!=result) {
                            storeType(key,result);
                        	existed = cache.get(key);
							if ((info.LastModified - existed.Timestamp).TotalMilliseconds < 1000) {
								return result;	
							}
                            
                        }
                    }
                    result = Compiler.CompileSingle(info, Options);
                    storeType(key,result);
                    return result;
                }
            }
            return existed.Type;
        
        }

        private void storeType(string key, Type type) {
            var last = type.getFirstAttribute<TimeStampAttribute>().GetDate();
            var item = new CompiledViewTypeItem {Key = key, Timestamp = last, Type = type};
            cache[key] = item;
        }

        protected  IDictionary<string ,CompiledViewTypeItem> cache = new Dictionary<string, CompiledViewTypeItem>();
    }
}
