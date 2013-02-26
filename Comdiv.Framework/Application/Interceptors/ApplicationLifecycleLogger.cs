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
using System.Web;
using Comdiv.Extensions;
using Comdiv.Logging;

namespace Comdiv.Application.Interceptors {
    public class ApplicationLifecycleLogger : IStartInterceptor, IStartApplicationInterceptor, IFinishInterceptor,
                                              IErrorInterceptor, IFinishApplicationInterceptor {
        private static readonly ILog htlog = logger.get("http");

        public void OnErrorRequest() {
            dolog("error");
        }

        public void OnFinishApplication() {
            dolog("appfinish");
        }

        public void OnFinishRequest() {
            dolog("finiish");
        }

        public void OnStartApplication() {
            dolog("appstart");
        }

        public void OnStartRequest() {
            dolog("start");
        }

        protected void dolog(string state) {
            if (htlog.IsDebugEnabled) {
                var url =
                    HttpContext.Current.Request.Url.ToString().find(@"\w+/\w+\.rails[\s\S]*$").Replace
                        (".rails", "");
                if (!(url.ToLower().Contains("/state.rails") || url.ToLower().Contains("/showerrors.rails"))) {
                    var usr = "NOUSR";
                    if (HttpContext.Current.User != null) {
                        usr = HttpContext.Current.User.Identity.Name;
                    }

                    var message =
                        string.Format("htlog stamp='{1}', usr='{0}', time='{4}', state='{3}', url='{2}', host='{5}'",
                                      usr,
                                      HttpContext.Current.Timestamp.ToString("HHmmssFFF"),
                                      url, state, DateTime.Now.ToString("HH:mm:ss"),
                                      HttpContext.Current.Request.UserHostAddress);
                    htlog.Debug(message);
                }
            }
        }
                                              }
}