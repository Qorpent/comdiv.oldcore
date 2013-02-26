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
using Comdiv.Extensions;
using Comdiv.Logging;

namespace Comdiv.Patching{
    public class DefaultPatch : PackageBase, IPatch{
        public string Dependencies { get; set; }

        #region IPatch Members

        public override IEnumerable<IPackageIdentity> GetDependences(){
            if (dependences.Count == 0 && Dependencies.hasContent()){
                var parents = Dependencies.Split(new[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parents){
                    dependences.Add(new SimplePackageIdentity(p));
                }
            }
            return dependences;
        }

        public virtual bool AutoLoad { get; set; }

        public virtual bool ReInstallable { get; set; }


        public virtual IPatchManager Manager { get; set; }

        public virtual bool IsInstalled{
            get { return Manager.IsInstalled(this); }
        }


        public override IPackageInstallResult Install(){
            try{
                var result = Install(Manager.PathResolver);
                if (result.State == ResultState.OK){
                    Manager.SetInstalledMark(this);
                }
                else{
                    if (result.Error != null){
                        throw result.Error;
                    }
                }
                return result;
            }
            catch (Exception ex){
                logger.get("comdiv.patching.patch").Error("error during installation of " + Code + " patch", ex);
                throw;
            }
        }

        public virtual string Code { get; set; }

        public string Comment { get; set; }

        #endregion
    }
}