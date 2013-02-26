using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Controllers{
    public class MeWorkspacePartition : WorkspacePartition{
        public MeWorkspacePartition(){
            ViewName = "/workspace/me";
            Role = WorkspaceZone.Top.ToString();
            Idx = 20;
        }

        private IInversionContainer _container;

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

        protected override void InternalExecute(){
            base.InternalExecute();
            this["currentUser"] = myapp.usrName;
            this["currentRole"] = myapp.roles.GetRoles().FirstOrDefault();
            
            this["roles"] = myapp.roles.GetRoles();
            
            this["impersonated"] = myapp.Impersonator.IsImpersonated(myapp.principals.BasePrincipal);
            this["isadmin"] = myapp.roles.IsAdmin();
            
        }
    }
}