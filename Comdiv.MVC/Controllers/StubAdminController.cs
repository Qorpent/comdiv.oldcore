using System.Linq;
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
    public class StubAdminController : Controller{
        public StubAdminController(){
            ControllerContext = new ControllerContext();
            // this.Context = new 
        }
    }

    [AdminAlways]
    public class StubController : Controller{
        public StubController(){
            ControllerContext = new ControllerContext();
        }
    }
}