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
    [Public]
    [Layout("workspace")]
    [Filter(ExecuteWhen.AfterAction, typeof (ExternalPartitionsFilter))]
    public class WorkspaceController : BaseController{
        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "вывод реестра пользователей"
            )]
        public virtual void Index(){
            myapp.storage.GetDefault().SetCacheMode(true);
            PropertyBag["getFileSystem"] = PathResolver;
        }
    }
}