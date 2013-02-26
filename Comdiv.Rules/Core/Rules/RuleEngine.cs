using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.Engine;
using Comdiv.Rules.Support;

namespace Comdiv.Rules{
    public class RuleEngine : RuleGroup{
        private IServicesContainer services;

        public RuleEngine(){
            IsContextBoundRulesHandler = true;
        }

        public RuleEngine(IEnumerable<IRule> rules) : base(rules){
            IsContextBoundRulesHandler = true;
        }

        public override IServicesContainer Services{
            get{
                return services ?? (services = new RuleServicesContainer(this))
                    ;
            }
            set { }
        }


        //TODO: Comdiv.Rules_CORE Приблизить логику поведения к базовой группе по использованию активаций
        //TODO: Comdiv.Rules_CORE Реализовать поддержку контекстно-привязанных правил
        protected override bool innerTest(IRuleContext context){
            InitContext(context);
            //return base.innerTest(context);
            return null != Rules.FirstOrDefault(r => r.Test(context));
        }

        protected override void innerInitContext(IRuleContext context){
            base.innerInitContext(context);

            executeTriggers(context, RuleExecutionPhase.InitContext);
        }

        public virtual void Execute(IRuleContext context, int steps){
            //@"context".contract_NotNull(context);
            //(0 < steps).contract_True("Число шагов должно быть больше 0");

            context.RuleData[this, Constants.Context.RuleData.EngineMaxSteps] = steps;
            Execute(context);
        }

        protected override void innerExecute(IRuleContext context){
            InitContext(context);
            Logger.Engine.Debug("START ENGINE {0}", Uid);
            PreRun(context);
            var executable = true;
            var actualSteps = 0;
            var availSteps = context.RuleData.Get(this, Constants.Context.RuleData.EngineMaxSteps, -1);
            while (executable){
                // проверяем контекст на наличие признака принудительного останова
                // в любом случае проверяется RuleData
                if (IsLocalScopeHalted(context)){
                    break;
                }
                // если продукция - внешняя то проверяется еще и глобальный признак останова
                if (IsOuterRule() && IsGlobalScopeHalted(context)){
                    break;
                }

                Logger.Engine.Debug("START STEP {0}:{1}", Uid, actualSteps + 1);
                PreStep(context);


                var activations = GetRawActivations(context);

                if (0 == activations.Count){
                    executable = false;
                }
                executeTriggers(context, RuleExecutionPhase.Test);
                if (executable){
                    Apply(activations, context);
                    executeTriggers(context, RuleExecutionPhase.Run);
                }

                PostStep(context);

                actualSteps++;
                if (-1 != availSteps && availSteps <= actualSteps){
                    executable = false;
                }
                Logger.Engine.Debug("END STEP {0}:{1}", Uid, actualSteps);
            }
            context.RuleData[this, Constants.Context.RuleData.EngineMaxSteps] = -1;
            PostRun(context);
            Logger.Engine.Debug("STOP ENGINE {0}", Uid);
        }

        protected bool IsGlobalScopeHalted(IRuleContext context){
            return context.Params.Get("sys.halt", false);
        }

        public bool IsOuterRule(){
            return null == Parent;
        }

        protected bool IsLocalScopeHalted(IRuleContext context){
            return context.RuleData.Get(this, Constants.Context.RuleData.EngineHalt, false);
        }

        private void executeTriggers(IRuleContext context, RuleExecutionPhase phase){
            foreach (var rule in Rules){
                if (rule is ITriggerCollection){
                    ((ITriggerCollection) rule).ExecuteTriggers(context, phase);
                }
            }
        }

        protected virtual void PreStep(IRuleContext context){
            executeTriggers(context, RuleExecutionPhase.PreStep);
        }

        protected virtual void PostRun(IRuleContext context){
            executeTriggers(context, RuleExecutionPhase.PostExpertRun);
        }

        protected virtual void PostStep(IRuleContext context){
            context.incCounter(this, "step");
            executeTriggers(context, RuleExecutionPhase.PostStep);
        }

        protected virtual void PreRun(IRuleContext context){
            executeTriggers(context, RuleExecutionPhase.PreExpertRun);
        }

        protected override bool preTest(IRuleContext context, out bool result){
            executeTriggers(context, RuleExecutionPhase.PreTest);
            return base.preTest(context, out result);
        }

        protected override void postExecute(IRuleContext context, bool result){
            base.postExecute(context, result);
            executeTriggers(context, RuleExecutionPhase.PostRun);
        }

        protected override void postTest(IRuleContext context, bool result){
            base.postTest(context, result);
            executeTriggers(context, RuleExecutionPhase.PostTest);
        }

        protected override bool preExecute(IRuleContext context){
            executeTriggers(context, RuleExecutionPhase.PreRun);
            return base.preExecute(context);
        }
    }
}