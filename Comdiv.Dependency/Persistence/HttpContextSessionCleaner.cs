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
using System.Linq;
using System.Web;
using Comdiv.Application;
using Comdiv.Inversion;
using NHibernate;

namespace Comdiv.Persistence {
    /// <summary>
    /// Перехватчик завершения веб-запроса.
    /// Закрывает и уничтожает текущии сессии запроса
    /// </summary>
    public class HttpContextSessionCleaner : IFinishInterceptor {
        private string[] _keys;
        private bool? hasfactory = null;
        protected string[] Keys {
            get {
                if(hasfactory==null) {
                    var fs = myapp.ioc.get<ISessionFactoryProvider>();
                    hasfactory = fs != null;
                    if(hasfactory.Value) {
                        _keys = fs.GetIds().Select(x=>"current.session."+x).ToArray();
                    }
                }
                return _keys;
            }
            
        }


        #region IApplicationOnFinishInterceptor Members
        
        public void OnFinishRequest() {
            if(Keys==null)return;
            //работает только с HttpContext;
            if (HttpContext.Current == null) return;
            var items = HttpContext.Current.Items;
            foreach (var key in Keys) {
                if(!items.Contains(key)) continue;
                var session = HttpContext.Current.Items[key] as ISession;
                if (null == session) continue;
                //to avoid case that session actually not exists
                HttpContext.Current.Items.Remove(key);
                //to prevent reference keeping

                session.Clear();
                if (session.Transaction != null) {
                    if (session.Transaction.IsActive) {
                        session.Transaction.Rollback();
                    }
                }
                if (session.IsOpen) {
                    session.Close();
                }
                session.Dispose();
            }
        }

        #endregion
    }
}