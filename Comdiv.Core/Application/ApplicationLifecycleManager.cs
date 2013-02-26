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
using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Common;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Application {
    public class ApplicationLifecycleManager : IApplicationLifecycleManager, IWithContainer {
        private IInversionContainer __container;
        private IApplicationInterceptor[] interceptors;

        public ApplicationLifecycleManager() {
            myapp.OnReload += myapp_OnReload;
        }

        protected IApplicationInterceptor[] Interceptors {
            get { return interceptors; }
            private set { interceptors = value; }
        }
        [TestPropose]
        public void SetupInterceptors(IEnumerable<IApplicationInterceptor> ints) {
            ints = ints ?? new IApplicationInterceptor[] {};
            this.interceptors = ints.ToArray();
        }

        

        #region IApplicationLifecycleManager Members

        private IStartInterceptor[] _startinterceptors;
        public void OnStartRequest() {
            lock (this) {
                var i = _startinterceptors ?? (_startinterceptors =  getinterceptors().OfType<IStartInterceptor>().ToArray());
                for (int _iter = 0; _iter < (i).Length; _iter++) {
                    var x = (i)[_iter];
                    x.OnStartRequest();
                }
            }
        }
        private IFinishInterceptor[] _finishinterceptors;
        public void OnFinishRequest() {
            lock (this) {
                var i = _finishinterceptors?? (_finishinterceptors = getinterceptors().OfType<IFinishInterceptor>().ToArray());
                for (int _iter = 0; _iter < (i).Length; _iter++) {
                    var x = (i)[_iter];
                    x.OnFinishRequest();
                }
            }
        }
        private IErrorInterceptor[] _errorinterceptors;
        public void OnErrorRequest() {
            lock (this) {
                var i =_errorinterceptors ?? (_errorinterceptors = getinterceptors().OfType<IErrorInterceptor>().ToArray());
                for (int _iter = 0; _iter < (i).Length; _iter++) {
                    var x = (i)[_iter];
                    x.OnErrorRequest();
                }
            }
        }
         private IPostAuthorizeInterceptor[] _postauthjorizeinterceptors;
        public void OnPostAuthorizeRequest() {
            lock (this) {
                var i = _postauthjorizeinterceptors ?? (_postauthjorizeinterceptors = getinterceptors().OfType<IPostAuthorizeInterceptor>().ToArray());
                for (int index = 0; index < i.Length; index++) {
                    var x = i[index];
                    x.OnPostAuthorizeRequest();
                }
            }
        }
        private IFinishApplicationInterceptor[] _finishapplicationinterceptors;
        public void OnFinishApplication() {
            lock (this) {
                var i = _finishapplicationinterceptors ?? (_finishapplicationinterceptors = getinterceptors().OfType<IFinishApplicationInterceptor>().ToArray());
                for (int index = 0; index < i.Length; index++) {
                    var x = i[index];
                    x.OnFinishApplication();
                }
            }
        }
        private IStartApplicationInterceptor[] _startapplicationinterceptors;
        public void OnStartApplication() {
            lock(this) {
                var i = _startapplicationinterceptors ?? (_startapplicationinterceptors = getinterceptors().OfType<IStartApplicationInterceptor>().ToArray());
                for (int k = 0; k < i.Length; k++) {
                    var x = i[k];
                    x.OnStartApplication();
                }
  
            }
        }

        #endregion

        #region IWithContainer Members

        public virtual IInversionContainer Container {
            get {
                lock (this) {
                    if (__container == null) {
                        __container = myapp.ioc;
                    }
                    return __container;
                }
            }
            set {
                lock (this) {
                    __container = value;
                }
            }
        }

        #endregion

        private void myapp_OnReload(object sender, EventWithDataArgs<int> args) {
            lock(this) {
                Interceptors = null;
                _startinterceptors = null;
                _startapplicationinterceptors = null;
                _finishinterceptors = null;
                _finishapplicationinterceptors = null;
                _postauthjorizeinterceptors = null;
                _errorinterceptors = null;
            }
        }

        

        private IApplicationInterceptor[] getinterceptors() {
            lock (this) {
                if (null == interceptors) {
                    interceptors = Container.all<IApplicationInterceptor>().ToArray();
                }
                return interceptors;
            }
        }
    }
}