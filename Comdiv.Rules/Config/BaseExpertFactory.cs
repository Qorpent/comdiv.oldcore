using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comdiv;
using Comdiv.Extensions;
using Comdiv.Rules;
using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Config{
    public class BaseExpertFactory : IExpertFactory{
        protected IKnowlegeBase createdExpert;
        private IList<ServiceDescriptor> engineServices = new List<ServiceDescriptor>();
        private IList<IRuleProvider> ruleProviders = new List<IRuleProvider>();
        private IList<object> ruleSources = new List<object>();
        private IList<IRuleTranslator> ruleTranslators = new List<IRuleTranslator>();
        private bool singleton = true;

        public string DefaultEngineTypeName{
            get{
                if (null == DefaultEngineType){
                    return null;
                }
                return DefaultEngineType.AssemblyQualifiedName;
            }
            set{
                if (string.IsNullOrEmpty(value)){
                    DefaultEngineType = null;
                }
                DefaultEngineType = Type.GetType(value, true);
            }
        }

        public Type DefaultEngineType { get; set; }

        public IList<IRuleProvider> RuleProviders{
            get { return ruleProviders; }
            set { ruleProviders = value; }
        }

        public IList<IRuleTranslator> RuleTranslators{
            get { return ruleTranslators; }
            set { ruleTranslators = value; }
        }

        public bool Singleton{
            get { return singleton; }
            set { singleton = value; }
        }

        public IList<ServiceDescriptor> EngineServices{
            get { return engineServices; }
            set { engineServices = value; }
        }


        public IRule OptimizerExpert { get; set; }

        public IList<object> RuleSources{
            get { return ruleSources; }
            set { ruleSources = value; }
        }

        #region IExpertFactory Members

        public IRule CreatePredefinedExpert(){
            if (Singleton && null != createdExpert){
                return createdExpert;
            }
            //		@"DefaultEngineType".contract_NotNull(DefaultEngineType,
            //                                  "Целевой тип движка должен быть установлен до вызова конструктора");
            var engine = DefaultEngineType.create<IKnowlegeBase>();

            //		@"engine".contract_NotNull(engine, "Не удалось инстанцировать базовый класс");


            if (engine is IWithServices){
                {
                    foreach (var service in EngineServices){
                        service.RegisterService(((IWithServices) engine).Services);
                    }
                }

                foreach (var provider in RuleProviders){
                    provider.SetupRules(engine);
                }

                foreach (var rule in TranslateToRules(RuleSources)){
                    engine.Rules.Add(rule);
                }
            }
            if (Singleton){
                createdExpert = engine;
            }
            return engine;
        }

        public IRule GetOptimizedVersion(IRule rule){
            if (null == OptimizerExpert){
                return rule;
            }
            var c = new ContextBase();
            c.Params["preOptimizedExpert"] = rule;
            OptimizerExpert.Execute(c);
            return c.Params.Get<IRule>("optimizedExpert");
        }

        public IList<IRule> TranslateToRules(IEnumerable ruleSources){
            var result = new List<IRule>();

            foreach (var translator in ruleTranslators){
                var subresult = translator.TranslateRules(ruleSources);
                if (null != subresult){
                    result.AddRange(subresult);
                }
            }

            return result;
        }

        #endregion
    }
}