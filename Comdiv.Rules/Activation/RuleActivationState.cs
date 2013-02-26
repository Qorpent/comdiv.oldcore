

namespace Comdiv.Rules.Activation{
    public class RuleActivationState{
        public RuleActivationState() {}

        public RuleActivationState(RuleActivationStateType type){
            Type = type;
        }

        public RuleActivationState(RuleActivationStateType type, int value){
            Type = type;
            Value = value;
        }

        public RuleActivationStateType Type { get; set; }

        public int Value { get; set; }

        public static RuleActivationState Always(){
            return new RuleActivationState(RuleActivationStateType.Always);
        }

        public static RuleActivationState Never(){
            return new RuleActivationState(RuleActivationStateType.Never);
        }

        public static RuleActivationState ActiveVersion(int v){
            return new RuleActivationState(RuleActivationStateType.ActiveVersion, v);
        }

        public static RuleActivationState PassiveVersion(int v){
            return new RuleActivationState(RuleActivationStateType.NonActiveVersion, v);
        }
    }
}