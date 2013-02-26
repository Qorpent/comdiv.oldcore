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
using System.Reflection;
using System.Security.Principal;
using Comdiv.Collections;
using Comdiv.Common;

using Comdiv.Design;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Persistence;
using Comdiv.Security;

namespace Comdiv.Application {
    ///<summary>
    ///</summary>
    /// 
    
    public static partial class myapp {
        
        private static IWidgetRepository _widgets;

        private static ISqlService _sql;


        public static Assembly ExtensionsAssembly { get; set; }

        public static Dictionary<string, object> ExtensionsRegistry { get; set; }

        public static IWidgetRepository widgets {
            get {
                lock(sync) {
                    if(_widgets==null) {
                        _widgets = Container.get<IWidgetRepository>() ?? new WidgetRepository{XmlReader = new ApplicationXmlReader()};
                    }
                    return _widgets;    
                }
                
            }
            set { _widgets = value; }
        }

        public static ISqlService sql {
            get {
                lock(sync) {
                    if(_sql == null) {
                        _sql = ioc.get<ISqlService>() ?? new SqlService();
                    }
                    return _sql;    
                }
                
            }
            set {
                lock (sync) {
                    _sql = value;
                }
            }
        }

        static Pool<IProfileRepository> profilePool = new Pool<IProfileRepository>();
        public static IProfileRepository getProfile(IPrincipal user = null) {
            lock(sync) {
            	return (IProfileRepository) myapp.ioc.get<IDefaultProfileRepository>();
            }
        }
        public static void release(IProfileRepository repository) {
            profilePool.Return(repository);
        }

        
        [TestPropose]
        public static void clearReloads() {
            lock(sync) {
                onReloadList.Clear();
            }
        }
    }
}