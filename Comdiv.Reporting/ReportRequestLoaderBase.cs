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
using Comdiv.Application;
using Comdiv.Inversion;


namespace Comdiv.MVC.Report{
    public abstract class ReportRequestLoaderBase : IReportRequestLoader{
        #region IReportRequestLoader Members

        public IReportRequest Load(string uid, IDictionary<string, object> advancedParameters,
                                   IMvcContext callingContext){
            try{
                return innerLoad(uid, advancedParameters, callingContext);
            }
            catch (Exception ex){
                throw new ReportLoadException("Report loading fails", ex, advancedParameters, uid);
            }
        }

        #endregion

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
        protected abstract IReportRequest innerLoad(string uid, IDictionary<string, object> parameters,
                                                    IMvcContext callingContext);
    }
}