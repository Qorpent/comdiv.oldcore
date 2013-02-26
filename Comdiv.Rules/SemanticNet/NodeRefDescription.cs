using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rules.SemanticNet{
    public class NodeRefDescription{
        private readonly double delta;
        private readonly string description;
        private readonly string nodeName;

        public NodeRefDescription(string nodeName, double delta, string description){
            this.delta = delta;
            this.description = description;
            this.nodeName = nodeName;
        }

        public double Delta{
            get { return delta; }
        }

        public string Description{
            get { return description; }
        }

        public string NodeName{
            get { return nodeName; }
        }

        public static NodeRefDescription Create(string nodeName, double delta, string description){
            return new NodeRefDescription(nodeName, delta, description);
        }
    }
}