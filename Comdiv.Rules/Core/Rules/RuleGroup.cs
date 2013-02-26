using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comdiv.Extensions;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;
using Comdiv.Rules.KnowlegeBase;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Support;
using Comdiv;

namespace Comdiv.Rules{
    [Flags]
    public enum DefaultGroupActivationBehaviour{
        All = 0,
        First = 1,
    }

    public class RuleGroup : RuleTemplated, IKnowlegeBase{
        private IGetActivationsModule getActivationModule;
        private GetActivationsDelegate getActivationOverride;
        private IRuleResolver resolver;
        private IRuleCollection rules;


        public RuleGroup() {}

        public RuleGroup(IEnumerable<IRule> rules){
            //@"rules".contract_NotNull(rules);
            AddRules(rules);
        }

        public IRuleResolver Resolver{
            get{
                if (resolver != null){
                    return resolver;
                }
                var service = Services.Get<IRuleResolver>();
                if (null != service){
                    return service;
                }
                return null;
            }
            set { resolver = value; }
        }

        public GetActivationsDelegate GetActivationOverride{
            get { return getActivationOverride; }
            set { getActivationOverride = value; }
        }

        public IGetActivationsModule GetActivationModule{
            get{
                if (getActivationModule != null){
                    return getActivationModule;
                }
                if (null == Services){
                    return null;
                }
                var service = Services.Get<IGetActivationsModule>();
                if (null != service){
                    return service;
                }
                return null;
            }
            set { getActivationModule = value; }
        }

        public bool IsContextBoundRulesHandler{
            get { return Params.Get(Constants.Meta.IsContextBoundHandler, true); }
            set { Params[Constants.Meta.IsContextBoundHandler] = value; }
        }

        public DefaultGroupActivationBehaviour ActivationBehaviour { get; set; }

        public virtual object DefaultResult{
            get { return Params.Get<object>("DefaultResult", Missing.Value); }
            set { Params["DefaultResult"] = value; }
        }

        #region IKnowlegeBase Members

        public IRuleCollection Rules{
            get { return rules ?? (rules = new RuleCollectionBase(this)); }
            set { rules = value; }
        }

        #endregion

        public void AddRule(IRule rule){
            //@"rule".contract_NotNull(rule);
            Rules.Add(rule);
        }

        public void AddRules(IEnumerable<IRule> rules){
            //@"rules".contract_NotNull(rules);
            foreach (var rule in rules){
                Rules.Add(rule);
            }
        }

        protected void Apply(IList<IRule> currentActivations, IRuleContext context){
            currentActivations = ResolveConflicts(currentActivations);
            foreach (var rule in currentActivations){
                rule.Execute(context);
            }
        }

        protected IList<IRule> ResolveConflicts(IList<IRule> currentActivations){
            if (currentActivations.Count > 1)
                //если требуется разрешение конфликтов
            {
                Logger.Kb.Debug("START RESOLVE {0} : {1}", Uid,
                                      string.Join(", ", currentActivations.Select(r => r.Uid()).ToArray()));
                if (null == Resolver)
                    //если не установлен разрешатель конфликтов
                {
                    currentActivations = new[]{currentActivations[0]};
                    //используем стратегию "взять первое попавшееся"
                }
                else{
                    currentActivations = Resolver.Filter(null, currentActivations);
                }
                //иначе передаем управление резольверу
                Logger.Kb.Debug("END RESOLVE {0} : {1}", Uid,
                                      string.Join(", ", currentActivations.Select(r => r.Uid()).ToArray()));
            }
            return currentActivations;
        }

        public virtual IList<IRule> GetRawActivations(IRuleContext context){
            //@"context".contract_NotNull(context);

            var ruleset = GetActiveRuleset(context);
            //получаем список активных правил (предпроверка правил, без входа в тело теста)
            return GetActivations(context, ruleset);
            //возвращаем набор протестированных правил
        }

        protected IList<IRule> GetActivations(IRuleContext context, IList<IRule> ruleset){
            Logger.Kb.Debug("START ACTIVATION CHECK {0}", Uid);
            if (null != GetActivationOverride){
                ruleset = getActivationOverride(ruleset, context);
            }
                //если установлен делегат, пользуемся им
            else if (null != GetActivationModule){
                ruleset = GetActivationModule.Filter(context, ruleset);
            }
                //если модуль активации, то им
            else{
                if (DefaultGroupActivationBehaviour.All == ActivationBehaviour){
                    ruleset = ruleset.Where(rule => rule.Test(context)).ToList();
                }
                else{
                    var first = ruleset.FirstOrDefault(rule => rule.Test(context));
                    if (first.yes()){
                        ruleset = new[]{first};
                    }
                    else{
                        ruleset = new IRule[]{};
                    }
                }
            }
            //иначе возвращаем все правила, для которых выполняется левая часть
            Logger.Kb.Debug("STOP ACTIVATION CHECK {0}", Uid);
            return ruleset;
        }


        protected IList<IRule> GetActiveRuleset(IRuleContext context){
            var ruleset = GetMyRules(context);
            //по умолчанию считаем все правила активными

            var activationService = null == Services ? null : Services.Get<IRuleActivationService>();
            if (null != activationService){
                ruleset = activationService.Filter(context, ruleset);
            }
            //но если есть сервис проверки активности правил, то пользуемся его услугами
            return ruleset.OrderByDescending(rule => rule.Priority()).ToList();
        }

        protected IList<IRule> GetMyRules(IRuleContext context){
            var localContextRules =
                context.RuleData.Get<IList<IRule>>(this, Constants.Context.BoundedRules, null);
            IList<IRule> globalContextRules = null;
            if (IsContextBoundRulesHandler){
                globalContextRules = context.Params.Get<IList<IRule>>(Constants.Context.BoundedRules);
            }
            if (null == localContextRules && null == globalContextRules){
                return Rules;
            }

            var result = new List<IRule>(Rules);
            if (null != localContextRules){
                result.AddRange(localContextRules);
            }
            if (null != globalContextRules){
                result.AddRange(globalContextRules);
            }
            return result;
        }


        protected override bool innerTest(IRuleContext context){
            if (EmptyKB()){
                return false;
            }
            //пустая база знаний - группа неактивна
            var rawactivations = GetRawActivations(context);
            if (rawactivations.Count == 0){
                return false;
            }
            //если нет ни одного активного правила то и группа не активна
            SetRawActivations(context, rawactivations);
            //сохраняем активные праивла в контексте
            return true;
            //раз есть активные правила значит и группа активна
        }

        protected object SetRawActivations(IRuleContext context, IList<IRule> rawactivations){
            return context.RuleData[this, Constants.Context.RuleData.GroupRawActivations] = rawactivations;
        }


        protected bool EmptyKB(){
            return 0 == Rules.Count;
        }

        protected override void innerExecute(IRuleContext context){
            var currentActivations = ExtractActivations(context);
            //извлекаем активные правила из контекста
            if (null == currentActivations){
                currentActivations = GetActivations(context, GetRawActivations(context));
            }
            //отсутствие активации в контексте означает, что группа выполняется в соло-режиме, без проверки, заново повторяем операции получения активных правил
            if (0 != currentActivations.Count){
                Apply(currentActivations, context);
            }
            //если есть активные правила, то применяем их контексту
        }

        protected IList<IRule> ExtractActivations(IRuleContext context){
            return context.RuleData.Get<IList<IRule>>(this, Constants.Context.RuleData.GroupRawActivations);
        }

        protected override void innerInitContext(IRuleContext context){
            foreach (var rule in Rules){
                if (rule is IContextInitiator){
                    ((IContextInitiator) rule).InitContext(context);
                }
            }
        }

        public void ApplyAll(IRuleContext context){
            var activations = this.GetRawActivations(context);
            foreach(var rule in activations){
                rule.Execute(context);
            }
        }

        protected void UseLinearOneStepPattern(){
            SetupCountHints(1, 1);
            ActivationBehaviour = DefaultGroupActivationBehaviour.First;
        }
        protected void UseAllApplyPattern()
        {
            SetupCountHints(0, 1);
            ActivationBehaviour = DefaultGroupActivationBehaviour.All;
        }
    }
}