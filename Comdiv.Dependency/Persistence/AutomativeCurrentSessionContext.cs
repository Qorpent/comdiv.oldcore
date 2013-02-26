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
using System.Threading;
using System.Web;
using Comdiv.Logging;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using NHibernate.Impl;

namespace Comdiv.Persistence {
    /// <summary>
    /// Источник текущей сессии, читает либо с потока, либо с Http, либо с временного ытатического поля потока
    /// </summary>
    public class AutomativeCurrentSessionContext : CurrentSessionContext {
        [ThreadStatic] internal static ISession temporary;
		[ThreadStatic]	public static ISession reached;
        private readonly ISessionFactoryImplementor factory;
        private readonly string theKey;
        public ILog log = logger.get("comdiv.persistence.currentsession");

        public AutomativeCurrentSessionContext(ISessionFactoryImplementor factoryImplementor) {
            factory = factoryImplementor;
            theKey = "current.session." + factory.Settings.SessionFactoryName;
        }


        protected override ISession Session {
            get {
                lock (this) {
                   ISession result = null;
                    if (temporary != null) {
                        return reached = temporary;
                    }
                    result = GetHttpContextSession();
                    if (null != result) {
                        return reached = result;
                    }
                    return reached = GetLocalThreadSession();
                }
            }
            set { temporary = value; }
        }
       
        private ISession GetLocalThreadSession() {
            lock (this) {
                var slot = Thread.GetNamedDataSlot(theKey);
                var result = Thread.GetData(slot) as ISession;
                if (null == result || !result.IsOpen || ((SessionImpl) result).Factory != factory) {
                    Thread.SetData(slot, result = newSession());
                }

                return result;
            }
        }

        private ISession GetHttpContextSession() {
            var httpCurrent = HttpContext.Current;
            ISession result = null;
            if (null != httpCurrent) {
                if (!httpCurrent.Items.Contains(theKey)) {
                    result = newSession();
                    httpCurrent.Items[theKey] = result;
                }
                else {
                    result = (ISession) httpCurrent.Items[theKey];
                    if (!result.IsOpen) {
                            httpCurrent.Items[theKey] = result = newSession();
                        
                    }
                }
            }
            return result;
        }


        private ISession newSession() {
            var result = factory.OpenSession();
            result.FlushMode = FlushMode.Never;
            return result;
        }
    }
}