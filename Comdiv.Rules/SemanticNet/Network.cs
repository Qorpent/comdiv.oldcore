using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Support;

namespace Comdiv.Rules.SemanticNet{
    public class Network{
        public const double DefaultTreshHold = 0.05;
        private readonly Dictionary<string, Node> byName = new Dictionary<string, Node>();
        private readonly Dictionary<string, List<Node>> byType = new Dictionary<string, List<Node>>();
        private readonly List<Node> simpleIndex = new List<Node>();
        private ParametersContainer<Network> @params;
        private double treshHold = DefaultTreshHold;

        public double TreshHold{
            get { return treshHold; }
            set { treshHold = value; }
        }

        public Node this[string name]{
            get { return GetNode(name); }
        }

        public IParametersProvider<Network> Params{
            get { return @params ?? (@params = new ParametersContainer<Network>()); }
        }

        public bool Exists(string nodeName){
            return byName.ContainsKey(nodeName);
        }

        public void AddNode(string value, string type, double startWeight, params NodeRefDescription[] causes){
            if (Exists(value)){
                throw new InvalidOperationException("Нельзя дважды добавить один и тот же узел");
            }
            var n = new Node{Value = value, Type = type, StartWeight = startWeight, ContainingNetwork = this};
            if (null != causes){
                foreach (NodeRefDescription caus in causes){
                    var srcnode = GetNode(caus.NodeName);


                    if (null == srcnode){
                        throw new InvalidOperationException("Попытка утвердить узел на основе не существующего");
                    }

                    var nodeRef = new Link(caus.Delta, srcnode, n, caus.Description);
                    srcnode.Out.Add(nodeRef);
                    n.In.Add(nodeRef);
                }
            }

            if (!byType.ContainsKey(n.Type)){
                byType[n.Type] = new List<Node>();
            }
            byType[n.Type].Add(n);
            byName[n.Value] = n;
            simpleIndex.Add(n);
        }

        public void AddNode(string value, double startWeight, params NodeRefDescription[] causes){
            AddNode(value, "none", startWeight, causes);
        }

        public void AddNode(string value, params NodeRefDescription[] causes){
            AddNode(value, 1, causes);
        }

        public void AddNode(string value){
            AddNode(value, null);
        }

        public Node GetNode(string value){
            if (!Exists(value)){
                return null;
            }
            return byName[value];
        }

        public IList<Node> GetNodes(string type){
            if (!byType.ContainsKey(type)){
                return new List<Node>();
            }
            return byType[type];
        }

        public IList<Node> AllNodes(){
            return simpleIndex;
        }

        public IList<Node> RootNodesList(){
            return new List<Node>(RootNodes());
        }

        public IEnumerable<Node> RootNodes(){
            foreach (Node node in simpleIndex){
                if (0 == node.In.Count){
                    yield return node;
                }
            }
        }

        public IList<Node> LeafNodesList(){
            return new List<Node>(LeafNodes());
        }

        public IEnumerable<Node> LeafNodes(){
            foreach (Node node in simpleIndex){
                if (0 == node.Out.Count){
                    yield return node;
                }
            }
        }

        public void CreateOrDeltaNode(string fromNode, string toNode){
            CreateOrDeltaNode(fromNode, toNode, GetNode(fromNode).Type, 1, "none", true);
        }

        public void CreateOrDeltaNode(string fromNode, string toNode, double delta){
            CreateOrDeltaNode(fromNode, toNode, GetNode(fromNode).Type, delta, "none", true);
        }

        public void CreateOrDeltaNode(string fromNode, string toNode, string toType, double delta, string description,
                                      bool generateOnNonExists){
            if (!Exists(fromNode)){
                throw new InvalidOperationException("Попытка продельтировать исходя из неизвестного узла");
            }
            if (!generateOnNonExists && !Exists(toNode)){
                throw new InvalidOperationException("Попытка продельтировать неизвестный узел");
            }
            if (generateOnNonExists && !Exists(toNode)){
                AddNode(toNode, toType, 0, NodeRefDescription.Create(fromNode, delta, description));
            }
            else{
                LinkNodes(fromNode, toNode, delta, description);
            }
        }

        public void LinkNodes(string fromNode, string toNode, double delta, string description){
            var from = GetNode(fromNode);
            if (null == from){
                throw new InvalidOperationException("Входного узла не существует");
            }
            var to = GetNode(toNode);
            if (null == to){
                throw new InvalidOperationException("Выходного узла не существует");
            }
            var nodeRef = new Link(delta, from, to, description);
            from.Out.Add(nodeRef);
            to.In.Add(nodeRef);
        }

        protected void ResetCount(){
            foreach (Node node in simpleIndex){
                node.WeightCounted = false;
            }
        }

        protected List<Node> GetLogicalRootsList(){
            return new List<Node>(GetLogicalRoots());
        }

        protected List<Node> GetNonCountedList(){
            return new List<Node>(GetNonCounted());
        }

        protected IEnumerable<Node> GetNonCounted(){
            foreach (Node node in simpleIndex){
                if (!node.WeightCounted){
                    yield return node;
                }
            }
        }

        protected IEnumerable<Node> GetLogicalRoots(){
            foreach (Node node in simpleIndex){
                if (node.WeightCounted){
                    continue;
                }
                if (0 == node.In.Count){
                    yield return node;
                }
                else{
                    var hasnotcounted = false;
                    foreach (Link nodeRef in node.In){
                        if (!nodeRef.Source.WeightCounted){
                            hasnotcounted = true;
                        }
                    }
                    if (!hasnotcounted){
                        yield return node;
                    }
                }
            }
        }

        public void CalculateWeights(){
            ResetCount();
            SortNodesByPreceding();
            foreach (Node node in simpleIndex){
                node.CalculateWeight();
            }
            IList<Node> roots;
            while (0 != ((roots = GetLogicalRootsList()).Count)){
                foreach (Node root in roots){
                    root.CalculateWeight();
                }
            }
            var notcounted = GetNonCountedList();
            if (null != notcounted && 0 != notcounted.Count){
                AdvancedWeightCalculations(notcounted);
            }
        }

        private void SortNodesByPreceding(){
            simpleIndex.Sort(
                delegate(Node n1, Node n2)
                    {
                        var n1c = n1.IsCauseFor(n2);
                        var n2c = n1.IsCauseFor(n1);
                        if (n1c && !n2c){
                            return -1;
                        }
                        if (!n1c && n2c){
                            return 1;
                        }
                        return 0;
                    }
                );
        }

        private static void SortNodesByBasisLevelAndPrecedence(List<Node> nodes){
            nodes.Sort(
                delegate(Node n1, Node n2)
                    {
                        var b1 = 0;
                        var b2 = 0;
                        foreach (Link nodeRef in n1.In){
                            if (nodeRef.Source.WeightCounted){
                                b1++;
                            }
                        }
                        foreach (Link nodeRef in n1.In){
                            if (nodeRef.Source.WeightCounted){
                                b2++;
                            }
                        }
                        if (b1 < b2){
                            return 1;
                        }
                        if (b2 > b1){
                            return -1;
                        }

                        if (n1.StartWeight > n2.StartWeight){
                            return -1;
                        }
                        if (n1.StartWeight < n2.StartWeight){
                            return 1;
                        }


                        if (n1 == n2){
                            return 0;
                        }
                        var n1c = n1.IsCauseFor(n2);
                        var n2c = n2.IsCauseFor(n1);
                        if (n1c && !n2c){
                            return -1;
                        }
                        if (!n1c && n2c){
                            return 1;
                        }


                        return 0;
                    }
                );
        }

        protected static void CalculateWeightByIns(Node node){
            CalculateWeightByIns(node, true, true, false, 0);
        }

        protected void AdvancedWeightCalculations(List<Node> notcounted){
            foreach (Node node in notcounted){
                node.CalculateWeight(false, true);
            }
            SortNodesByBasisLevelAndPrecedence(notcounted);
            var hasChanges = true;
            while (hasChanges){
                hasChanges = false;
                foreach (Node node in notcounted){
                    hasChanges = node.CalculateWeightCascaded(new List<Node>(), TreshHold) || hasChanges;
                }
            }
        }


        protected static void CalculateWeightByIns(Node node, bool useStart, bool useCounted, bool useNonCounted,
                                            double nonCountedCoef){
            var weight = useStart ? node.StartWeight : node.Weight;
            bool wasNoCounted;
            weight = GetWeight(node, useCounted, weight, useNonCounted, nonCountedCoef, out wasNoCounted);
            if (!wasNoCounted){
                node.WeightCounted = true;
            }
            node.Weight = weight;
        }

        private static double GetWeight(Node node, bool useCounted, double weight, bool useNonCounted, double nonCountedCoef,
                                 out bool wasNoCounted){
            wasNoCounted = false;
            foreach (var nodeRef in node.In){
                if (nodeRef.Source.WeightCounted){
                    if (useCounted){
                        weight += nodeRef.Source.Weight*nodeRef.Delta;
                    }
                }
                else{
                    wasNoCounted = true;
                    if (useNonCounted){
                        weight += nodeRef.Source.Weight*nodeRef.Delta*nonCountedCoef;
                    }
                }
            }
            return weight;
        }
    }
}