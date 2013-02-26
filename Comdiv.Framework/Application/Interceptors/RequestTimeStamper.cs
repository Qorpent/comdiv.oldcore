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
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Comdiv.Application.Interceptors {
    public class RequestTimeStamper : IStartInterceptor, IFinishInterceptor {
        private const string ITEMKEY = "RequestTimeStamper_stopwatch";

        public void OnStartRequest() {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Items[ITEMKEY] = Stopwatch.StartNew();
        }

        public void OnFinishRequest() {
            if (HttpContext.Current == null) return;
            var sw = HttpContext.Current.Items[ITEMKEY] as Stopwatch;
            HttpContext.Current.Items.Remove(ITEMKEY);
            var type = HttpContext.Current.Response.ContentType;
            if (!type.StartsWith("text/")) return;
            if (null != sw) {
                sw.Stop();
                var matrix = "<p> work time : {0} t, {1} ms<p>";
                if (type.EndsWith("javascript") || type.Contains("css")) {
                    matrix = "/* work time : {0} t, {1} ms*/";
                }
                if (HttpContext.Current.Request.Params.AllKeys.OfType<string>().FirstOrDefault(x => x == "ajax") == null) {
                    HttpContext.Current.Response.Write(string.Format(matrix, sw.ElapsedTicks, sw.ElapsedMilliseconds));
                }
            }
        }
    }
}