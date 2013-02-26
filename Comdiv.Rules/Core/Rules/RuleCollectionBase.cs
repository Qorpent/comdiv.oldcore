using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Support{
    public class RuleCollectionBase : List<IRule>, IRuleCollection{
        public RuleCollectionBase() {}

        public RuleCollectionBase(IRule containingRule){
            ContainingRule = containingRule;
        }

        #region IRuleCollection Members

        public int Version { get; protected set; }

        public IRule ContainingRule { get; set; }


        public new void Add(IRule rule){
            //@"rule".contract_NotNull(rule);
            Version = Version + 1;
            base.Add(rule);
            rule.Parent = ContainingRule;
        }

        public new bool Remove(IRule rule){
            //@"rule".contract_NotNull(rule);
            Version = Version + 1;
            if (ContainingRule == rule.Parent){
                rule.Parent = null;
            }
            return base.Remove(rule);
        }

        #endregion
    }
}