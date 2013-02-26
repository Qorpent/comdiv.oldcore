using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Support;

namespace Comdiv.Rules.SemanticNet{
    public class Link : IWithParameters<Link>{
        private ParametersContainer<Link> @params;

        public Link(double delta, Node srcnode, Node trgnode, string description){
            this.Delta = delta;
            Source = srcnode;
            this.Description = description;
            Target = trgnode;
        }

        public double Delta { get; set; }

        public Node Source { get; set; }

        public string Description { get; set; }

        public int Fired { get; set; }

        public double PreviousValue { get; set; }

        public Node Target { get; set; }

        public string Type { get; set; }

        #region IWithParameters<Link> Members

        public IParametersProvider<Link> Params{
            get { return @params ?? (@params = new ParametersContainer<Link>()); }
        }

        #endregion

        public override string ToString(){
            return string.Format("{0}->{1} {2} //{3}", Source.Value, Target.Value, Delta, Description);
        }
    }
}