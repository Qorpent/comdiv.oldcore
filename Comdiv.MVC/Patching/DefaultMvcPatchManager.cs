using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Comdiv.Distribution;

using Comdiv.IO;
using Comdiv.IO;

namespace Comdiv.MVC.Patching{
    public class DefaultMvcPatchManager : IPatchManager{
        private IList<IMvcPatch> patches;
        public DefaultMvcPatchManager(){
            this.TargetFileSystem = this._<IApplicationFileSystem>();
            this.FileSystem = this.TargetFileSystem.GetSubSystem("patch");
        }
        #region IPatchManager Members

        public IApplicationFileSystem TargetFileSystem { get; set; }

        public IEnumerable<IMvcPatch> Patches{
            get{
                if (null == patches){
                    LoadPatches();
                }
                return patches;
            }
        }

        protected virtual void LoadPatches() {
            patches = new List<IMvcPatch>();
            var patchIocFile = TargetFileSystem.ReadXml("ioc/patches.config", null,
                                                        new ReadXmlOptions{Merge = true});
            if (patchIocFile == null){
                patchIocFile = "<configuration />";
            }
            try{
                var patchIocResource = new StaticContentResource(patchIocFile);
                if (null != patchIocFile){

                    var container = new WindsorContainer(new XmlInterpreter(patchIocResource));
                    try{
                        foreach (var patch in container.ResolveAll<IMvcPatch>()){
                            patch.FileSystem = FileSystem;
                            patch.Manager = this;
                            patch.Identity = new SimplePackageIdentity(patch.Code);
                            patches.Add(patch);
                        }
                    }
                    finally{
                        container.Dispose();
                    }
                }
            }catch(Exception ex){
                throw new Exception("error in config:\r\n"+patchIocFile,ex);
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
            foreach(var patch in Patches.Where(p=>p.ReInstallable))
            {
                TargetFileSystem.FileDelete("usr/patch/.inst/" + patch.Code + ".installed");
            }
        }

        public IFileSystem FileSystem { get; set; }

        public void SetInstalledMark(IMvcPatch patch){
            TargetFileSystem.Write("usr/patch/.inst/" + patch.Code + ".installed", "");
        }

        public bool IsInstalled(IMvcPatch patch){
            return TargetFileSystem.FileExists("usr/patch/.inst/" + patch.Code + ".installed");
        }

        public void ApplyStartupPathces(){
            foreach (var startupPatch in Patches.Where(p => p.AutoLoad)){
                doInstall(startupPatch);
            }
        }

        #endregion

        public void Install(string patchCode){
            var patch = (IMvcPatch) Load(patchCode);
            if(null!=patch)doInstall(patch);
        }

        private void doInstall(IMvcPatch patch){
            
            foreach (var identity in patch.GetDependences()){
                doInstall((IMvcPatch) Load(identity));
            }
            if (patch.IsInstalled){
                return;
            }
            logger.MvcUsing.InfoFormat("INSTALLING PATCH: {0}...",patch.Code);
            patch.Install();
        }

        public void Reload(){
            patches = null;
        }


    }
}