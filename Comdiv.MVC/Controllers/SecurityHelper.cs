using System.Linq;
using System.Security.Principal;
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
using Qorpent.Security;

namespace Comdiv.MVC.Controllers{
    public class SecurityHelper{
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
        public IPrincipal User{
            get { return Container.get<IPrincipalSource>().CurrentUser; }
        }

        public string UserName{
            get { return myapp.usrName; }
        }

        public string Role{
            get{
                return myapp.roles.GetRoles().FirstOrDefault();
            }
        }

        public bool isa(string role){
            return myapp.roles.IsInRole(role);
        }

        public bool isin(params string[] roles){
            foreach (var role in roles){
                if (isa(role)){
                    return true;
                }
            }
            return false;
        }
    }
}