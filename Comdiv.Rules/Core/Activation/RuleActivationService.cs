using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;
using Comdiv.Rules.Support;

namespace Comdiv.Rules.Activation{
    public class RuleActivationService : List<IRuleActivationChecker>, IRuleActivationService{
        #region IRuleActivationService Members

        public IList<IRule> Filter(IRuleContext context, IList<IRule> rules){
            var result = new List<IRule>();
            //TODO: Реализовать логику с кэшированием всех значений, пока отрабатываются только Never и Always

            foreach (var rule in rules){
                if (context.RuleData.Get(rule, "sys.activation.never", false)){
                    continue;
                }
                if (context.RuleData.Get(rule, "sys.activation.always", false)){
                    result.Add(rule);
                    continue;
                }
                var alwaysCount = 0;
                var never = false;
                var locked = false;
                foreach (var checker in this){
                    var state = checker.GetActivationState(context, rule);
                    switch (state.Type){
                        case RuleActivationStateType.Always:
                            alwaysCount++;
                            goto case RuleActivationStateType.Active;
                        case RuleActivationStateType.ActiveVersion:
                            goto case RuleActivationStateType.Active;
                        case RuleActivationStateType.Active:
                            break;
                        case RuleActivationStateType.Never:
                            locked = true;
                            never = true;
                            break;
                        default:
                            locked = true;
                            break;
                    }
                    if (locked){
                        break;
                    }
                }
                if (never){
                    context.RuleData[rule, "sys.activation.never"] = true;
                }
                if (alwaysCount == Count){
                    context.RuleData[rule, "sys.activation.always"] = true;
                }
                if (!locked){
                    result.Add(rule);
                }
            }

            return result;
        }

        #endregion

        public static void AddCheker(IRule rule, IRuleActivationChecker checker){
            AddCheker(rule, checker, true);
        }

        public static void AddCheker(IRule rule, IRuleActivationChecker checker, bool createServiceOnNotExisted){
            //	//@"rule".contract_NotNull(rule);
            //	@"checker".contract_NotNull(checker);

            GetActivationSet(
                GetServices(rule),
                createServiceOnNotExisted
                )
                .Add(checker);
        }

        protected static IWithServices GetServices(IRule rule){
            var services = RuleServicesContainer.GetServicesContainer(rule);
            //@"services".contract_NotNull(services,
            //                            "Представлена продукция без доступа к контейнеру сервисов, не могу завершить операцию");
            //@"services.Services".contract_NotNull(services.Services, "Контейнер сервисов не инициализирован");

            return services;
        }

        protected static IRuleActivationService GetActivationSet(IWithServices services, bool createServiceOnNotExisted){
            var activationSet = services.Services.Get<IRuleActivationService>();

            //CONTRACT
            ///	(null != activationSet || createServiceOnNotExisted).contract_True(
            //		"Служба активации не представлена и создание новой запрещено");
            //ENDCONTRACT

            if (null == activationSet){
                activationSet = new RuleActivationService();
                services.Services.RegisterService(activationSet);
            }
            return activationSet;
        }
    }
}