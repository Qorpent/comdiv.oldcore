using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public abstract class AbstractModuleLoader : RuleTemplated{
        public AbstractModuleLoader() {}


        public AbstractModuleLoader(IList<string> modules){
            Modules = modules;
        }

        public AbstractModuleLoader(IList<string> modules, bool activate){
            Modules = modules;
            IsActivator = activate;
        }

        public bool IsActivator{
            get { return Params.Get("sf.moduleloader.activator", true, true); }
            set { Params["sf.moduleloader.activator"] = value; }
        }

        public IList<string> Modules{
            get { return Params.Get<IList<string>>("sf.moduleloader.modules", new string[]{}, true); }
            set { Params["sf.moduleloader.modules"] = value; }
        }

        protected override void innerInitContext(IRuleContext context){
            IModuleService s = context.modules();
            if (null == s && context is IWithServices){
                ModuleService service = new ModuleService();
                ((IWithServices) context).Services.RegisterService<IModuleService>(service);
            }
            base.innerInitContext(context);
        }

        protected override void innerExecute(IRuleContext context){
            foreach (string module in Modules){
                IModuleService ms = context.modules();
                ms.ChangeActivation(module, IsActivator);
            }
        }

        protected override bool preTest(IRuleContext context, out bool result){
            result = true;
            if (Modules.Count == 0){
                result = false;
            }
            bool hasdifferent = false;
            foreach (string s in Modules){
                IModuleService ms = context.modules();
                if (IsActivator != ms.IsActive(s)){
                    hasdifferent = true;
                    break;
                }
            }
            if (!hasdifferent){
                result = false;
            }
            if (!result){
                return false;
            }
            return base.preTest(context, out result);
        }
    }
}