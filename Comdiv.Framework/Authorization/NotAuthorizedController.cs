using Castle.MonoRail.Framework;
using Comdiv.Controllers;

namespace Comdiv.Authorization {
    [Layout("default")]
    public class NotAuthorizedController : BaseController {
        public void show(string name, string action, string user) {
            PropertyBag["name"] = name;
            PropertyBag["action"] = action;
            PropertyBag["user"] = user;
        }
    }
}