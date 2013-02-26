using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Support;

namespace Comdiv.Rules.Config{
    [Obsolete]
    public class ExpertBuilder{
        public static void InitExpertCoreServices(IWithServices target){
            //@"target".contract_NotNull(target);

            if (null == target.Services){
                target.Services = new ServicesContainer();
            }
            var activations = new RuleActivationService();
            var modules = new ModuleActivationChecker();
            modules.AlwaysActive.Add("default");
            activations.Add(new PassiveGroupsChecker());
            activations.Add(new SelfTestedActivationChecker());
            activations.Add(modules);
            activations.Add(new HintCounterChecker());
            target.Services.RegisterService<IRuleActivationService>("rule.activations", activations);
        }

        public static IRule BuildExpertWithCoreServices(){
            var result = new RuleEngine();
            InitExpertCoreServices(result);
            return result;
        }

        public static IRuleContext BuildContextWithCoreServices(){
            IRuleContext result = new ContextBase();
            InitCoreContextServices(result);
            return result;
        }

        public static void InitCoreContextServices(IWithServices target){
            if (null == target.Services){
                target.Services = new ServicesContainer();
            }
            target.Services.RegisterService<IModuleService>("context.modules", new ModuleService());
        }

        public static void InitCoreContextServices(IRuleContext target){
            if (target is IWithServices){
                InitCoreContextServices((IWithServices) target);
            }
        }
    }
}