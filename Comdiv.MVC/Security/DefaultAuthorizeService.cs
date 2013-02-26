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
using Qorpent.Security;

namespace Comdiv.MVC.Security{
    public interface IAuthorizeService{
        bool Authorize(IMvcContext descriptor);
        bool Authorize(string key, bool defaultValue, IMvcContext descriptor);
    }

    public class DefaultAuthorizeService : IAuthorizeService{
        public DefaultAuthorizeService(){
            RoleResolver = myapp.roles;
            ;
        }

        public IRoleResolver RoleResolver { get; set; }

        #region IAuthorizeService Members

        public bool Authorize(IMvcContext descriptor){
            lock (this){
                return Authorize("authorize", descriptor);
            }
        }


        public bool Authorize(string key, bool defaultValue, IMvcContext descriptor){
            lock (this){


                if (myapp.roles.IsAdmin()){
                    return true;
                }
                return acl.get(descriptor, false);
            }
        }

        #endregion

        public bool Authorize(string key, IMvcContext descriptor){
            lock (this){
                return Authorize(key, true, descriptor);
            }
        }
    }
}