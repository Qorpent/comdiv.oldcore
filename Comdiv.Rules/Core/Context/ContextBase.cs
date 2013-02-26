using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.Support;

namespace Comdiv.Rules{
    public class ContextBase : IRuleContext, IWithServices{
        private IDocumentProvider docs;
        private IParametersProvider<IRuleContext> @params;
        private IRuleDataProvider ruleData;
        private IServicesContainer services;

        public ContextBase() {}

        public ContextBase(IDocumentProvider docs, IRuleDataProvider ruleData, IParametersProvider<IRuleContext> @params){
            this.docs = docs;
            this.ruleData = ruleData;
            this.@params = @params;
        }

        public IList<IRule> ContextBoundRules{
            get{
                if (null == Params[Constants.Context.BoundedRules,null]){
                    Params[Constants.Context.BoundedRules] = new RuleCollectionBase();
                }
                return Params.Get<IList<IRule>>(Constants.Context.BoundedRules);
            }
        }

        #region IRuleContext Members

        public IDocumentProvider Docs{
            get { return docs ?? (docs = new DocumentProviderBase(this)); }
            set{
                docs = value;
                docs.ContainingContext = this;
            }
        }

        public IRuleDataProvider RuleData{
            get { return ruleData ?? (ruleData = new RuleDataProviderBase()); }
            set { ruleData = value; }
        }

        public IParametersProvider<IRuleContext> Params{
            get { return @params ?? (@params = new RuleContextPartametersContainer(this)); }
            set{
                @params = value;
                @params.Target = this;
            }
        }

        #endregion

        #region IWithServices Members

        public IServicesContainer Services{
            get{
                return services ?? (services = new ServicesContainer())
                    ;
            }
            set { services = value; }
        }

        #endregion
    }
}