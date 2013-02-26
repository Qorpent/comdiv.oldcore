using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;
using Comdiv.Reporting;

namespace Comdiv.MVC.Rules{
    public class ControllerRuleStorage : DefaultStorage{
        public ControllerRuleStorage(){
            UseIocOnPipelineConstruction = false;
        }

        protected override void preparePipeline(){
            base.preparePipeline();
            Pipeline.Add(new ControllerRuleStorageExecutor());
        }
    }
}