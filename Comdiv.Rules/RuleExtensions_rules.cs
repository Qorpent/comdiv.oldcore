using System;
using System.Collections.Generic;
using System.Globalization;
using Comdiv.Rules.Engine;
using Comdiv.Rules;

namespace Comdiv.Rules
{
    public static partial class RuleExtensions
    {
        public static string Uid(this IRule rule){
            //@"rule".contract_NotNull(rule);
            var result = rule.Params.Get<string>(Constants.Meta.Uid, null);
            if (string.IsNullOrEmpty(result)){
                result = rule.GetType().Name + Guid.NewGuid();
                rule.Params[Constants.Meta.Uid] = result;
            }
            return result;
        }

        public static IRule SetUid(this IRule rule, string value){
            //@"rule".contract_NotNull(rule);
            rule.Params[Constants.Meta.Uid] = value;
            return rule;
        }

        public static string Module(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return (string) rule.Params[Constants.Meta.Module, "default"];
        }

        public static int Priority(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return (int) rule.Params[Constants.Meta.Priority, 0];
        }

        public static IRule SetPriority(this IRule rule, int value){
            //@"rule".contract_NotNull(rule);
            rule.Params[Constants.Meta.Priority] = value;
            return rule;
        }

        public static IRule SetModule(this IRule rule, string module){
            //@"rule".contract_NotNull(rule);
            //@"module".contract_HasContent(module);
            rule.Params[Constants.Meta.Module] = module;
            return rule;
        }

        public static bool IsTrigger(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return (bool) rule.Params[Constants.Meta.IsTrigger, false];
        }

        public static bool IsTriggerForPhase(this IRule rule, RuleExecutionPhase phase){
            //@"rule".contract_NotNull(rule);
            if (rule.IsTrigger()){
                var phases_ = (string) rule.Params["sys.trigger.phases", ""];
                var phases = phases_.Split(',', '_', ';', ' ');
                var phase_ = phase.ToString();
                if (Array.IndexOf(phases, phase_) != -1)
                    return true;
            }
            return false;
        }

        public static IRule MakeTrigger(this IRule rule, RuleExecutionPhase phase, params RuleExecutionPhase[] phases){
            //@"rule".contract_NotNull(rule);
            rule.Params[Constants.Meta.IsTrigger] = true;
            var phases_ = new List<string>();
            phases_.Add(phase.ToString());
            if (phases != null){
                foreach (var p in phases)
                    phases_.Add(p.ToString());
            }

            rule.Params["sys.trigger.phases"] = string.Join(";", phases_.ToArray());
            return rule;
        }

        public static bool IsWithCountHints(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return (bool) rule.Params[Constants.Meta.IsWithCountHints, false];
        }

        public static IRule SetupCountHints(this IRule rule, int maxBads, int maxExecs){
            //@"rule".contract_NotNull(rule);
            //((-1 == maxBads) || (1 <= maxBads)).contract_True(string.Format(CultureInfo.InvariantCulture,
            //                                                                "Неверное число неудачных проверок {0}", maxBads));
            //((-1 == maxExecs) || (1 <= maxExecs)).contract_True(string.Format(CultureInfo.InvariantCulture,
            //                                                                "Неверное число выполнений {0}", maxExecs));

            rule.Params[Constants.Meta.IsWithCountHints] = true;
            rule.Params[Constants.Meta.IsWithCountHints + ".maxbad"] = maxBads;
            rule.Params[Constants.Meta.IsWithCountHints + ".exec"] = maxExecs;
            return rule;
        }

        public static int GetHintCounter(this IRule rule, string counter){
            //@"rule".contract_NotNull(rule);
            if (!IsWithCountHints(rule)) return -1;
            return (int) rule.Params[Constants.Meta.IsWithCountHints + "." + counter, -1];
        }

        public static int GetMaxBadTestCountHint(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return GetHintCounter(rule, "maxbad");
        }

        public static int GetMaxExecCountHint(this IRule rule){
            //@"rule".contract_NotNull(rule);
            return GetHintCounter(rule, "exec");
        }
    }
}