using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Comdiv.Rules;
using Comdiv.Rules.Activation;

namespace Comdiv.Rules.Activation{
    public class RegularExpressionBasedFriendshipMatcher : IFriendshipMatcher{
        private readonly string regex;

        public RegularExpressionBasedFriendshipMatcher(string regex){
            this.regex = regex;
        }

        #region IFriendshipMatcher Members

        public bool IsFirend(IRule friendCandidate, IRule main){
            var ruleidentity = (string) main.Params[Constants.Meta.CRFriendshipIdentityString, null];
            if (ruleidentity == null){
                return false;
            }
            return Regex.Match(ruleidentity, regex, RegexOptions.Compiled).Success;
        }

        #endregion
    }
}