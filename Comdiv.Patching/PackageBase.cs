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
using Comdiv.IO;

namespace Comdiv.Patching{
    public class PackageBase : IPackage{
        protected readonly IList<IPackageIdentity> dependences = new List<IPackageIdentity>();
        private readonly IList<IPackageInstallTask> tasks = new List<IPackageInstallTask>();

        private IPackageIdentity identity;

        #region IPackage Members

        public virtual IList<IPackageInstallTask> Tasks{
            get { return tasks; }
        }

        public virtual IPackageIdentity Identity{
            get { return identity ?? new SimplePackageIdentity(); }
            set { identity = value; }
        }

        public virtual IPackageInstallResult Install(){
            return Install(PathResolver);
        }

        public virtual IPackageInstallResult Install(IFilePathResolver targetSystem){
            var result = new DefaultPackageInstallResult();
            // Console.WriteLine("INFO : {0} start install",identity);
            foreach (var task in Tasks){
                // try{
                //  Console.WriteLine("INFO : {0}/{1} start ", identity,task);
                var subResult = task.Do(this, targetSystem);
                //   Console.WriteLine("INFO : {0}/{1} finish ", identity, task);
                result.SubResults.Add(subResult);
                if (subResult.State > ResultState.Warning){
                    break;
                }
                // }
                //catch (Exception e){
                //   result.Error = e;
                //   result.State = ResultState.Error;
                //}
            }
            Console.WriteLine("INFO : {0} end install", identity);
            return result;
        }

        public virtual IEnumerable<IPackageIdentity> GetDependences(){
            return dependences;
        }

        public virtual IFilePathResolver PathResolver { get; set; }

        #endregion

        public void AddDependency(IPackageIdentity identity){
            dependences.Add(identity);
        }
    }
}