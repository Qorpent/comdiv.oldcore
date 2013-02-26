using System;
using Castle.MonoRail.Framework;

namespace Comdiv.Controllers {
    public class RescueController:SmartDispatcherController {
        public void NotAuthorized(Exception exception, IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException("RescueController->NotAuthorized, sfo");
        }
    }
}