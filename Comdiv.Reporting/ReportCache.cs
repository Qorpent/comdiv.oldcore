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
using System.Collections.Generic;
using System.IO;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;
using Comdiv.Reporting;

namespace Comdiv.Reporting{
    public class ReportCache : IReportCache{
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
        public ReportCache(){
            keyprovider = Container.get<IReportDefinitionCacheStringProvider>() ?? new ReportDefinitionCacheStringProvider();
            checkers = Container.all<IReportCacheLeaseChecker>().ToList();
        }
        private IFilePathResolver fs = myapp.files;
        private IReportDefinitionCacheStringProvider keyprovider = null;
        private IList<IReportCacheLeaseChecker> checkers = null;
        public string Get(IReportDefinition definition){
            var filename = keyprovider.GetCacheString(definition).toSystemName();
            var fname = fs.Resolve("~/tmp/reports/" + filename,false);
            //если нет кэшированной версии, возвращаем null
            if(!File.Exists(fname)){
                return null;
            }
            var time = File.GetLastWriteTime(fname);
            //проверяем лицензию
            foreach (IReportCacheLeaseChecker checker in checkers){
                //если хоть один провайдер лицензий дает отклик false - вернуть Null
                if(!checker.IsValid(definition,time)){
                    return null;
                }
            }
            return File.ReadAllText(fname);
        }
        public void Set(IReportDefinition definition,string content){
            var filename = keyprovider.GetCacheString(definition).toSystemName();
            fs.Write("~/tmp/reports/" + filename, content);
        }

        public void Clear(){
            var dir = fs.Resolve("~/tmp/reports",false);
            if(Directory.Exists(dir)){
                Directory.Delete(dir);
            }
        }
    }
}