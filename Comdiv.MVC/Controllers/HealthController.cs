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
    [ControllerDetails("health", Sessionless = true, Area = "sys")]
    public class HealthController : SmartDispatcherController{
        [ActionDescription(ActionRole.Public, ActionSeverity.NonCritical, "Проверяет общую работоспособность системы")]
        public void test(){
            RenderText("I can show it<br/>it means that application can load IoC and run monorail");
        }
    }
}