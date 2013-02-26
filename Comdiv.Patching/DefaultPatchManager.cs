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
using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;

namespace Comdiv.Patching{
    public class DefaultPatchManager : IPatchManager{
        private IInversionContainer _container;
        private IFilePathResolver _pathResolver;
        private IFilePathResolver _targetPathResolver;
        private IList<IPatch> patches;

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

        public IFilePathResolver TargetPathResolver{
            get { return _targetPathResolver ?? Container.get<IFilePathResolver>(); }
            set { _targetPathResolver = value; }
        }

        #region IPatchManager Members

        public IEnumerable<IPatch> Patches{
            get{
                if (null == patches){
                    LoadPatches();
                }
                return patches;
            }
        }

        public IPackageStoringResult Save(IPackage package){
            throw new NotImplementedException();
        }

        public IPackage Load(IPackageIdentity identity){
            return Patches.FirstOrDefault(p => p.Code == identity.Name);
        }

        public IPackage Load(string packageName){
            return Patches.FirstOrDefault(p => p.Code == packageName);
        }

        public void DropInstallationMarks(){
            foreach (var patch in Patches.Where(p => p.ReInstallable)){
                TargetPathResolver.Delete("usr/patch/.inst/" + patch.Code + ".installed");
            }
        }

        public IFilePathResolver PathResolver{
            get{
                lock (this){
                    if (null == _pathResolver){
                        _pathResolver = new DefaultFilePathResolver{
                                                                       Root =
                                                                           ((ILayeredFilePathResolver)
                                                                            TargetPathResolver).Root,
                                                                       DefaultPrefix = "patch"
                                                                   };
                    }
                    return _pathResolver;
                }
            }
            set{
                //meet interface but prevent to define it by ioc
            }
        }

        public void SetInstalledMark(IPatch patch){
            TargetPathResolver.Write("usr/patch/.inst/" + patch.Code + ".installed", "");
        }

        public bool IsInstalled(IPatch patch){
            return TargetPathResolver.Exists("usr/patch/.inst/" + patch.Code + ".installed");
        }

        public void ApplyStartupPathces(){
            foreach (var startupPatch in Patches.Where(p => p.AutoLoad)){
                doInstall(startupPatch);
            }
        }

        public void Install(string patchCode){
            var patch = (IPatch) Load(patchCode);
            if (null != patch){
                doInstall(patch);
            }
        }

        #endregion

        protected virtual void LoadPatches(){
            lock (this){
                patches = new List<IPatch>();
                var patchIocFile = TargetPathResolver.ReadXml("ioc/patches.config", null,
                                                              new ReadXmlOptions{Merge = true});
                if (patchIocFile == null){
                    patchIocFile = "<configuration />";
                }
                try{
                    var patchIocResource = new StaticContentResource(patchIocFile);
                    if (null != patchIocFile){
                        var container = new WindsorContainer(new XmlInterpreter(patchIocResource));
                        try{
                            foreach (var patch in container.ResolveAll<IPatch>()){
                                patch.PathResolver = PathResolver;
                                patch.Manager = this;
                                patch.Identity = new SimplePackageIdentity(patch.Code);
                                patches.Add(patch);
                            }
                        }
                        finally{
                            container.Dispose();
                        }
                    }
                }
                catch (Exception ex){
                    throw new Exception("error in config:\r\n" + patchIocFile, ex);
                }
            }
        }

        private void doInstall(IPatch patch){
            foreach (var identity in patch.GetDependences()){
                doInstall((IPatch) Load(identity));
            }
            if (patch.IsInstalled){
                return;
            }
            logger.get("comdiv.patching.manager").Info("INSTALLING PATCH: {0}...", patch.Code);
            patch.Install();
        }

        public void Reload(){
            patches = null;
        }
    }
}