using System;
using System.Linq;
using Comdiv.Distribution;
using System.Collections.Generic;
using Comdiv.Patching;


namespace Comdiv.MVC.Patching{
    public class MvcPatch : PackageBase, IMvcPatch{
        #region IMvcPatch Members

        public override IEnumerable<IPackageIdentity> GetDependences(){
            if(dependences.Count==0 && Dependencies.hasContent()){
                var parents = Dependencies.Split(new[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                foreach(var p in parents){
                    dependences.Add(new SimplePackageIdentity(p));
                }

            }
            return dependences;
        }
        
        public virtual bool AutoLoad { get; set; }

        public virtual bool ReInstallable { get; set;}


        

        public string Dependencies {
            get;
            set;
        }

        public virtual IPatchManager Manager { get; set; }

        public virtual bool IsInstalled{
            get { return Manager.IsInstalled(this); }
        }

        

        public override IPackageInstallResult Install(){
            try{
                var result = Install(Manager.TargetFileSystem);
                if(result.State==ResultState.OK){
                    Manager.SetInstalledMark(this);
                }else{
                    if(result.Error!=null){
                        throw result.Error;
                    }
                }
                return result;
            }
            catch (Exception ex){
                logger.MvcUsing.Error("error during installation of " + Code + " patch", ex);
                throw;
            }
        }

        public virtual string Code { get; set; }

        public string Comment{ get; set;}
       

        #endregion
    }
}