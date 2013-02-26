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
using System.Linq;

namespace Comdiv.Patching{
    public class DefaultPackageInstallResult : IPackageInstallResult{
        private readonly IList<string> subMessages = new List<string>();
        private readonly IList<IPackageInstallResult> subResults = new List<IPackageInstallResult>();

        private ResultState __state = ResultState.None;

        #region IPackageInstallResult Members

        public IList<IPackageInstallResult> SubResults{
            get { return subResults; }
        }

        public ResultState State{
            get{
                if (ResultState.None != __state){
                    return __state;
                }
                if (SubResults.Count != 0){
                    return SubResults.Select(r => r.State).Max();
                }
                return __state;
            }
            set { __state = value; }
        }

        public Exception Error { get; set; }

        public string Message { get; set; }

        public IList<string> SubMessages{
            get { return subMessages; }
        }

        #endregion

        public static IPackageInstallResult Ok(string message){
            return new DefaultPackageInstallResult{State = ResultState.OK, Message = message};
        }
    }
}