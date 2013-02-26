using System.Security;
using System.Threading;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Controllers;

namespace Comdiv.Security {
    public class ImpersonateController : BaseController
    {
        public void enter(string username) {
            if (myapp.roles.IsAdmin(myapp.principals.BasePrincipal)) {
                myapp.Impersonator.Impersonate(myapp.principals.BasePrincipal, username);
                RenderText("OK");
            }else {
                throw new SecurityException("Попытка имперсонации от имени не-административной учетной записи");
            }
        }
        public void leave()
        {
            myapp.Impersonator.DeImpersonate(myapp.principals.BasePrincipal);
            RenderText("OK");
        }
    }
}