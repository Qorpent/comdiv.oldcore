using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Support;

namespace Comdiv.Rules.SemanticNet{
    public class Node : IWithParameters<Node>{
        private IList<Link> @in;
        private IList<Link> @out;
        private ParametersContainer<Node> @params;
        private double startWeight;
        private bool weightCounted;

        public IList<Link> In{
            get { return @in ?? (@in = new List<Link>()); }
        }

        public IList<Link> Out{
            get { return @out ?? (@out = new List<Link>()); }
        }

        public string Value { get; set; }

        public string Type { get; set; }

        public double StartWeight{
            get { return startWeight; }
            set { startWeight = value; }
        }

        public double Weight { get; set; }

        public bool WeightCounted{
            get { return weightCounted; }
            set { weightCounted = value; }
        }

        public string Description { get; set; }

        protected internal double OldWeight { get; set; }

        public Network ContainingNetwork { get; internal set; }

        #region IWithParameters<Node> Members

        public IParametersProvider<Node> Params{
            get { return @params ?? (@params = new ParametersContainer<Node>()); }
        }

        #endregion

        public override string ToString(){
            return
                string.Format("Source: {0} [{1}] - {2}/{4}{5} // {3}", Value, Type, StartWeight,
                              Description, weightCounted ? "" : "?", Weight);
        }

        public bool CalculateWeightCascaded(List<Node> used, double treshHold){
            if (WeightCounted){
                return false;
            }
            double delta = Weight - OldWeight;
            if (OldWeight != 0 && Math.Abs(delta/OldWeight) <= treshHold){
                WeightCounted = true;
                return false;
            }
            OldWeight = Weight;

            bool proceed = true;
            if (used.Contains(this)){
                proceed = false;
            }
            else{
                used.Add(this);
            }
            if (proceed){
                foreach (Link nodeRef in Out){
                    nodeRef.Target.Weight += delta*nodeRef.Delta;
                    nodeRef.Target.CalculateWeightCascaded(used, treshHold);
                }
            }
            return true;
        }

        public bool IsCauseFor(Node targetNode){
            foreach (Link o in Out){
                if (targetNode == o.Target){
                    return true;
                }
            }
            return false;
        }


        public bool CalculateWeight(){
            return CalculateWeight(true);
        }

        public bool CalculateWeight(bool onlyRootMode){
            return CalculateWeight(onlyRootMode, false);
        }

        public bool CalculateWeight(bool onlyRootMode, bool applyAnyTime){
            if (WeightCounted){
                return true;
            }
            bool hasnocounted = false;
            double weight = startWeight;
            foreach (Link o in In){
                if (!o.Source.WeightCounted){
                    hasnocounted = true;
                    if (onlyRootMode){
                        break;
                    }
                }
                weight += o.Source.Weight*o.Delta;
            }
            if (!onlyRootMode || !hasnocounted || applyAnyTime){
                Weight = weight;
            }
            if (!hasnocounted){
                WeightCounted = true;
            }
            return WeightCounted;
        }
    }
}