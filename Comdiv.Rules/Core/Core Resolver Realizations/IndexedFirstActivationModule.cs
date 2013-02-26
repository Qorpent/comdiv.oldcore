using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public class IndexedFirstActivationModule : GetActivationsModule{
        private int index;

        public IEnumerable<int> GetIndexes(int stopIndex, IList<IRule> collection){
            //	@"collection".contract_NotNull(collection);

            if (index >= collection.Count){
                index = 0;
            }
            if (index != stopIndex - 1 && !(stopIndex == 0 && index == (collection.Count - 1))){
                yield return index++;
            }
        }


        public override IList<IRule> Filter(IRuleContext context, IList<IRule> ruleset){
            //@"context".contract_NotNull(context);
            //	@"ruleset".contract_NotNull(ruleset);

            foreach (var i in GetIndexes(index, ruleset)){
                if (ruleset[i].Test(context)){
                    return new[]{ruleset[i]};
                }
            }
            return null;
        }
    }
}