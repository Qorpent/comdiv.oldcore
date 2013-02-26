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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Model.Interfaces;

namespace MvcContrib.Comdiv.Wiki
{
    public interface IWikiFilter:IWithIdx{
        
        string Process(string current);
    }

    public class RegexWikiFilter:IWikiFilter{
        public int Idx { get; set; }

        public IDictionary<string, string> Replaces{
            get { return replaces; }
        }

        public RegexWikiFilter(string from, string to){
            Replaces[from] = to;
        }

        public RegexWikiFilter(IDictionary hash)
        {
            foreach (var key in hash){
                Replaces[key.ToString()] = hash[key].ToString();
            }
        }

        private IDictionary<string,string> replaces = new Dictionary<string, string>();
        public string Process(string current){
            foreach (var replace in replaces){
                current = current.replace(replace.Key,replace.Value);
            }
            return current;
        }
    }
    public class WikiResult:ActionResult
    {
        private IFilePathResolver _fileResolver;

        

        protected IWikiFilter[] filters { get; set; }

        public WikiResult(string filename){
            this.WikiFileName = filename;
        }
        public WikiResult(string viewname,string mastername)
            
        {
            this.ViewName = viewname;
            this.MasterName = mastername;
        }

        public string MasterName { get; set; }

        public string ViewName { get; set; }
        public string WikiFileName
        {
            get;
            set;
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

        public IFilePathResolver FileResolver{
            get { return _fileResolver ?? Container.get<IFilePathResolver>(); }
            set { _fileResolver = value; }
        }

        public override void ExecuteResult(ControllerContext context){
            string data = "";
            if(ViewName!=null){
                var eng = System.Web.Mvc.ViewEngines.Engines.OfType<IBrailViewFactory>().FirstOrDefault();
                var sw = new StringWriter();
                var view = eng.FindView(context, ViewName, MasterName, false);
                var ctx = new ViewContext(context, view.View, new ViewDataDictionary(), new TempDataDictionary(),sw);
                view.View.Render(ctx,sw);
                data = sw.ToString();
            }else{
                var file = FileResolver.Resolve("wiki/"+WikiFileName + ".wiki", true);
                if(null==file){
                    throw new Exception("wiki file for "+WikiFileName+" not found");
                }
                data = File.ReadAllText(file);

            }
            
            foreach (var filter in filters){
                data = filter.Process(data);    
            }

            context.HttpContext.Response.Write(data);
            context.HttpContext.Response.Flush();
        }
    }
}
