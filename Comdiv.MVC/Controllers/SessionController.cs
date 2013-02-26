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
    public class SessionController : BaseController{
        [Public]
        public void set(string name, string value){
            //Cache.Store(name + "ioc.get" + myapp.usrName, value);
            myapp.getProfile().Set(name,value);
            CancelView();
        }
    }
}