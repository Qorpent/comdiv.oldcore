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
    [ControllerDetails("config", Area = "sys")]
    public class ConfigurationController : BaseController{
        [Public]
        [ActionDescription(
            ActionRole.User,
            ActionSeverity.DataAccess,
            "получение строки конфигурации ресурса"
            )]
        public void Get(string path){
            RenderText(PathResolver.Read(path));
        }
    }
}