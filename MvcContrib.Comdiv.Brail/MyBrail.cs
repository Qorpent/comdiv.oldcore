using System;
using System.Collections.Generic;
using System.IO;
using Comdiv.Application;
using Comdiv.Extensions;
using MvcContrib.Comdiv.ViewEngines.Brail;
using MvcContrib.ViewFactories;

namespace MvcContrib.Comdiv.Brail{
    public class MyBrail{
        private static MyBrail _default;
        public BooViewEngine Engine { get; set; }
        public readonly IDictionary<string , string > Views = new Dictionary<string, string>();
        public MyBrail(){
            Engine = new BooViewEngine();
            Engine.Options = new MvcViewEngineOptions();
            Engine.Options.CommonScriptsDirectory = null;
            Engine.Options.SaveBooResult = false;
            Engine.Factory = new BrailViewFactory(Engine);
   
            myapp.OnReload += (s, a) => filecache.Clear();
            PrepareViewResolving();
        }

        public string ViewDirectory
        {
            get;
            set;
        }

        public static MyBrail Default{
            get{
                if (null == _default){
                    _default = new MyBrail();
                  
                }
                return _default;
            }
            set { _default = value; }
        }

        private void PrepareViewResolving(){
            this.Engine.Factory.OnResolveViewName += ((sender1, args1) => {
                                                          args1.Result = args1.Name.Replace(".brail", "");
                                                      });
            this.Engine.Factory.OnResolveCode += ((sender, args) =>{
                                                      if (Views.ContainsKey(args.Name)){
                                                          args.Result = Views[args.Name];
                                                          return;
                                                      }

                                                      var path = myapp.files.Resolve("views/" + args.Name + ".brail",true);
                                                      if(null!=path){
                                                          args.Result = File.ReadAllText(path);
                                                          return;
                                                      }
                                                        

                                                      throw new Exception("cannot resolve view "+args.Name);
                                                  });
        }

        public static string ProcessFile(string file){
            return ProcessFile(file, null);
        }

        public static string ProcessFile(string file, object viewdata){
            return Default.Engine.ProcessFile(file, viewdata);
        }

        IDictionary<string , string > filecache = new Dictionary<string, string>();
        public string processViewFile(string file,object viewdata){
            if (!filecache.ContainsKey(file)){
                var realfile = myapp.files.Resolve("views/" + file + ".brail", true);
                filecache[file] = File.ReadAllText(realfile);
            }
            return Process(filecache[file],viewdata);
        }

        public static string _Process(string code){
            return _Process(code, null);
        }

        public static string _Process(string code, object viewdata){
            return Default.Engine.ProcessCode(code, viewdata);
        }

        public string ProcessView(string viewname)
        {
            return Process(Engine.Factory.CustomResolveCode(viewname), null);
        }

        public string ProcessView(string viewname,object viewdata)
        {
            return Process(Engine.Factory.CustomResolveCode(viewname), viewdata);
        }

        public string Process(string code){
            return Process(code, null);
        }

        public string Process(string code, object viewdata){
            return Engine.ProcessCode(code, viewdata);  
        }

        public string Process(string code, object viewdata, string layout)
        {
            return Engine.ProcessCode(code, viewdata,layout);
        }
    }
}