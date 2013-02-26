using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Comdiv.Rules.TextAnalyzer{
    public class ExtractorOperation{
        private double delta = 1;

        public ExtractorOperation(string regex) : this(regex, 1) {}
        public ExtractorOperation(string regex, double delta) : this(regex, NodeApplyMode.CreateOnly, delta) {}

        public ExtractorOperation(string regex, NodeApplyMode mode) : this(regex, mode, 1) {}

        public ExtractorOperation(string regex, NodeApplyMode mode, double delta){
            this.Regex = regex;
            this.Mode = mode;
            this.delta = delta;
            
        }

        public ExtractorOperation(){
            
        }

        public string Regex { get; set; }

        public NodeApplyMode Mode { get; set; }

        public double Delta{
            get { return delta; }
            set { delta = value; }
        }

        public bool Activated { get; set; }

        public MatchCollection ResultCollection { get; protected set; }

        public bool Tested { get; set; }

        public void Apply(string text){
           
            Tested = true;
            if(string.IsNullOrEmpty(text)||string.IsNullOrEmpty(Regex))return;
            MatchCollection result = System.Text.RegularExpressions.Regex.Matches(text, Regex, RegexOptions.Compiled);
            if (null != result && 0 != result.Count){
                Activated = true;
                ResultCollection = result;
            }
        }

        public override bool Equals(object obj){
            if (ReferenceEquals(null, obj)){
                return false;
            }
            if (ReferenceEquals(this, obj)){
                return true;
            }
            if (obj.GetType() != typeof (ExtractorOperation)){
                return false;
            }
            return Equals((ExtractorOperation) obj);
        }

        public void Reset(){
            Tested = false;
            Activated = false;
            ResultCollection = null;
        }

        public bool Equals(ExtractorOperation other){
            if (ReferenceEquals(null, other)){
                return false;
            }
            if (ReferenceEquals(this, other)){
                return true;
            }
            return other.delta == delta && Equals(other.Regex, Regex) && Equals(other.Mode, Mode);
        }

        public override int GetHashCode(){
            unchecked{
                int result = delta.GetHashCode();
                result = (result*397) ^ (Regex != null ? Regex.GetHashCode() : 0);
                result = (result*397) ^ Mode.GetHashCode();
                return result;
            }
        }
    }
}