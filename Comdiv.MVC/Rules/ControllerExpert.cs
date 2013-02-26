using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Resources;
using Comdiv.Persistence;
using Comdiv.Rules.Context;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Rules;

namespace Comdiv.MVC.Rules{
    public static class ControllerExpert{
        private static bool? notAvail;
        private static StorageWrapper<IControllerExpert> storage;

        public static bool NotAvail{
            get{
                if (!notAvail.HasValue){
                    if (myapp.storage.Get<IControllerExpert>()==null){
                        logger.get("comdiv.mvc.using").Warn("Controller experts are not initiated!");
                        NotAvail = true;
                    }
                    else{
                        NotAvail = false;
                        storage = myapp.storage.Get<IControllerExpert>();
                    }
                }
                return notAvail.Value;
            }
            set { notAvail = value; }
        }

        public static object Run(string name, object defaultResult, IMvcContext descriptor){
            if (NotAvail){
                return defaultResult;
            }
            var expert = storage.Load(name);
            if (null == expert){
                return defaultResult;
            }
            var context = CreateContext(descriptor);
            if (expert.Test(context)){
                expert.Execute(context);
            }
            return context.ControllerRuleResult();
        }

        public static IRuleContext CreateContext(IMvcContext context){
            var result = new ContextBase();
            result.SetDescriptor(context);
            return result;
        }

        public static MvcContext Descriptor(this IRuleContext context){
            return context.Params.Get<MvcContext>(ControllerRuleStrings.Default.Context_MvcContext);
        }

        public static void SetDescriptor(this IRuleContext context, IMvcContext descriptor){
            context.Params[ControllerRuleStrings.Default.Context_MvcContext] = descriptor;
        }

        public static object ControllerRuleResult(this IRuleContext context){
            return context.Params.Get<object>(ControllerRuleStrings.Default.Context_Result);
        }

        public static object SetControllerRuleResult(this IRuleContext context, object value){
            return context.Params[ControllerRuleStrings.Default.Context_Result] = value;
        }
    }
}