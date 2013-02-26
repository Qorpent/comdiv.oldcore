using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Caching;
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

namespace Comdiv.MVC{
    public class DefaultMvcCache : ApplicationCacheBase{
        public DefaultMvcCache(){
            CriticalSize = 100;
            NormalSize = 50;
            DefaultLeaseTime = TimeSpan.FromMinutes(20);
        }

        protected override bool IsAuthorized(ICacheObject item){
            lock (this){


                //admins can see any request
                if (RoleResolver.IsAdmin()){
                    return true;
                }
                //null as owner - shared request
                if (null == item.Owner){
                    return true;
                }

                //else only user's sessions allowed
                return item.Owner == PrincipalSource.CurrentUser.Identity.Name;
            }
        }
    }
}