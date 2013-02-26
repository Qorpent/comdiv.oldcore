using System;
using System.Linq;
using System.Security;
using System.Security.Principal;
using Castle.MonoRail.Framework;
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
    [Admin]
    [ControllerDetails("impersonation", Area = "sys")]
    public class ImpersonationController : BaseController{
        public ImpersonationController(){
            Impersonator = Container.get<IImpersonator>();
        }

        public void UserList(){
            SelectedViewName = "/workspace/impersonatoruserlist";
        }

        public IImpersonator Impersonator { get; set; }

        [ActionDescription(ActionRole.Admin, ActionSeverity.SecurityLack, "Выводит форму имперсонации")]
        public void Index() {}

        [ActionDescription(ActionRole.Admin, ActionSeverity.SecurityLack, "выполняет имперсонацию")]
        public void Impersonate(string userName, string roleName){
            if (string.IsNullOrEmpty(roleName)){
                roleName = "DEFAULT";
            }
            if (roleName.ToLower() == "root"){
                redirectToError(new MvpException("cannot go to root by simple impersonation, only by promoteToRoot",
                                                 null));
                return;
            }

            Impersonator.Impersonate(myapp.usr, userName);
            RenderText("you was impersonated as {0} in role {1}", myapp.usrName,myapp.roles.GetRoles().FirstOrDefault());
        }

        [Public]
        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "перевход еще под одним именем"
            )]
        public void ReLogin(string userName, string roleName, string redirect){
            if (roleName.ToLower() == "root"){
                redirectToError(new MvpException("cannot go to root by simple impersonation, only by promoteToRoot",
                                                 null));
                return;
            }
            if (string.IsNullOrEmpty(roleName)){
                roleName = "DEFAULT";
            }
            myapp.Impersonator.DeImpersonate(myapp.usr);
            if (myapp.roles.IsAdmin()){
                Impersonator.Impersonate(myapp.usr, userName);
            }
            else{
                throw new SecurityException("not admin attempt to relogin");
            }
            if (redirect.hasContent()){
                if (redirect.StartsWith("/")){
                    RedirectToUrl(Context.ApplicationPath + redirect);
                }
                else{
                    RedirectToUrl(redirect);
                }
            }
            RenderText("you was impersonated as {0} in role {1}", myapp.usrName, myapp.roles.GetRoles().FirstOrDefault());
        }


        [ActionDescription(ActionRole.Admin, ActionSeverity.NonCritical, "выполняет деимперсонацию")]
        [Public]
        public void DeImpersonate(){
            myapp.Impersonator.DeImpersonate(myapp.principals.BasePrincipal);
           // myapp.principals.BasePrincipal = null;
            RenderText("you was deimpersonated and now again you are {0} in role {1}", myapp.usrName,
                       myapp.roles.GetRoles().FirstOrDefault());
        }


        public IPrincipal Resolve(IPrincipal user){
            throw new NotImplementedException();
        }
    }
}