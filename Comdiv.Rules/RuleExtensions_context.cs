using System.Collections.Generic;
using Comdiv.Rules.Context;
using Comdiv.Rules.Activation;

namespace Comdiv.Rules
{
    public static partial class RuleExtensions
    {
        public static string getCounterName(string name){
            //"name".contract_HasContent(name);

            return "sys.counter." + name;
        }

        public static void setCounter(this IRuleContext context, IRule rule, string counterName, int value){
            checkCounterContract(context, rule, counterName);

            context.RuleData[rule, getCounterName(counterName)] = value;
        }

        private static void checkCounterContract(IRuleContext context, IRule rule, string counterName){
            checkContextRuleContract(context, rule);
            //"counterName".contract_HasContent(counterName);
        }

        private static void checkContextRuleContract(IRuleContext context, IRule rule){
            //"context".contract_NotNull(context);
            //"rule".contract_NotNull(rule);
        }

        public static int getCounter(this IRuleContext context, IRule rule, string counterName){
            checkCounterContract(context, rule, counterName);
            return (int) context.RuleData[rule, getCounterName(counterName), 0];
        }

        public static void incCounter(this IRuleContext context, IRule rule, string counterName){
            checkCounterContract(context, rule, counterName);
            setCounter(context, rule, counterName, getCounter(context, rule, counterName) + 1);
        }

        public static void addTest(this IRuleContext context, IRule rule){
            incCounter(context, rule, "test");
        }

        public static void addBadTest(this IRuleContext context, IRule rule){
            incCounter(context, rule, "badtest");
        }

        public static void addExec(this IRuleContext context, IRule rule){
            incCounter(context, rule, "exec");
        }

        public static int badCount(this IRuleContext context, IRule rule){
            return getCounter(context, rule, "badtest");
        }

        public static int execCount(this IRuleContext context, IRule rule){
            return getCounter(context, rule, "exec");
        }

        public static IModuleService modules(this IRuleContext context){
            //"context".contract_NotNull(context);
            if (!(context is IWithServices)) return null;
            return ((IWithServices) context).Services.Get<IModuleService>();
        }

        public static IList<IRule> nonActiveRules(this IRuleContext context, IRule rule){
            return getRuleGroup(context, "nonactive", rule);
        }

        public static IList<IRule> workingRules(this IRuleContext context, IRule rule){
            return getRuleGroup(context, "work", rule);
        }

        public static IList<IRule> getRuleGroup(this IRuleContext context, string groupName, IRule rule){
            checkContextRuleContract(context, rule);
            //"groupname".contract_HasContent(groupName);

            var result = context.RuleData[rule, "sys.filtered_activations." + groupName] as IList<IRule>;
            if (null == result){
                result = new List<IRule>();
                context.RuleData[rule, "sys.filtered_activations." + groupName] = result;
            }
            return result;
        }
    }
}