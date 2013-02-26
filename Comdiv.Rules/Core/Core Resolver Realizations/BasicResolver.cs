using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;
using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Activation{
    public class BasicResolver : IRuleResolver{
        private bool supportPriority = true;


        public bool SupportPriority{
            get { return supportPriority; }
            set { supportPriority = value; }
        }

        public bool SupportSafeProduction { get; set; }

        public bool SupportFriendship { get; set; }

        #region IRuleResolver Members

        public IList<IRule> Filter(IRuleContext context, IList<IRule> rules){
            //@"rules".contract_NotNull(rules);
            if (rules == null || rules.Count == 0){
                return null;
            }
            if (rules.Count == 1){
                return new[]{rules[0]};
            }
            IRule mainTarget = null;
            IList<IRule> obligations = SupportSafeProduction ? new List<IRule>() : null;
            IList<IRule> friendship = SupportFriendship ? new List<IRule>() : null;

            foreach (var rule in rules){
                var safer = CheckSafeProduction(rule, obligations);
                if (safer){
                    continue;
                }

                var oldMainTarget = mainTarget;
                mainTarget = GetMainTarget(rule, mainTarget);
                if (SupportFriendship){
                    if (SupportFriendshipMatching(rule)){
                        friendship.Add(rule);
                    }
                }
            }

            if (SupportFriendship && friendship.Count != 0){
                IList<IRule> realFriendship = new List<IRule>();
                foreach (var rule in friendship){
                    if (mainTarget == rule){
                        continue;
                    }

                    var matcher_ = GetFriendshipMatcher(rule);
                    var matcher = matcher_ is IFriendshipMatcher
                                      ? (IFriendshipMatcher) matcher_
                                      : new RegularExpressionBasedFriendshipMatcher(matcher_.ToString());
                    rule.Params[Constants.Meta.CRFriendshipMatcher] = matcher;
                    if (matcher.IsFirend(rule, mainTarget)){
                        realFriendship.Add(rule);
                    }
                }
                friendship = realFriendship;
            }
            var result = new List<IRule>();
            result.Add(mainTarget);
            if (SupportFriendship){
                result.AddRange(friendship);
            }
            if (SupportSafeProduction){
                result.AddRange(obligations);
            }
            return result.ToArray();
        }

        #endregion

        public static void SetSafeProduction(IRule rule){
            //@"rule".contract_NotNull(rule);
            rule.Params[Constants.Meta.CRSafeProduction] = true;
        }

        public static void SetPriority(IRule rule, int priority){
            //@"rule".contract_NotNull(rule);
            rule.Params[Constants.Meta.Priority] = priority;
        }

        public void SetFriendshipIdentity(IRule rule, string identity){
            //@"rule".contract_NotNull(rule);
            //@"identity".contract_HasContent(identity);

            rule.Params[Constants.Meta.CRFriendshipIdentityString] = identity;
        }

        public void SetFriendshipMatcher(IRule rule, string regex){
            //@"rule".contract_NotNull(rule);
            //@"regex".contract_HasContent(regex);

            SetFriendshipMatcher(rule,
                                 new RegularExpressionBasedFriendshipMatcher(regex));
        }

        public void SetFriendshipMatcher(IRule rule, IFriendshipMatcher matcher){
            //@"rule".contract_NotNull(rule);
            //@"matcher".contract_NotNull(matcher);

            rule.Params[Constants.Meta.CRFriendshipMatcher] = matcher;
        }

        private bool SupportFriendshipMatching(IRule checkFriends){
            return null != GetFriendshipMatcher(checkFriends);
        }

        private object GetFriendshipMatcher(IRule checkFriends){
            return checkFriends.Params[Constants.Meta.CRFriendshipMatcher, null];
        }

        private IRule GetMainTarget(IRule rule, IRule mainTarget){
            if (
                mainTarget == null || (SupportPriority && (Priority(mainTarget) < Priority(rule)))){
                mainTarget = rule;
            }
            return mainTarget;
        }

        protected bool CheckSafeProduction(IRule rule, IList<IRule> rules){
            if (SupportSafeProduction && IsSafeProduction(rule)){
                rules.Add(rule);
                return true;
            }
            return false;
        }

        protected bool IsSafeProduction(IRule rule){
            return (bool) rule.Params[Constants.Meta.CRSafeProduction, false];
        }

        protected int Priority(IRule mainTarget){
            return (int) mainTarget.Params[Constants.Meta.Priority, 0];
        }
    }
}