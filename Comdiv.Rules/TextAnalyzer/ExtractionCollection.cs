using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.SemanticNet;

namespace Comdiv.Rules.TextAnalyzer{
    public class ExtractionCollection : List<ExtractionDescription>{
        public const string ExtractorsNodeMeta = "ta.extractors";
        public const string PrimaryNodeMeta = "ta.isprimary";

        public ExtractionCollection() {}

        public ExtractionCollection(int capacity) : base(capacity) {}

        public ExtractionCollection(IEnumerable<ExtractionDescription> collection) : base(collection) {}

        public void Optimize(){
            var subst = new List<ExtractionDescription>(this);
            Clear();
            AddRange(Optimized(subst));
        }

        public void UpdateNetwork(Network net){
            DoUpdateNetwork(net, GetActiveResult());
        }

        public static void DoUpdateNetwork(Network net, IEnumerable<ExtractionDescription> descriptions){
            foreach (ExtractionDescription description in descriptions){
                if (net.Exists(description.ResultNodeName)){
                    if (description.CanBeDelted){
                        UpdateNetworkWithDelta(net, description);
                    }
                }
                else{
                    if (description.CanBeCreated){
                        UpdateNetworkWithCreate(net, description);
                    }
                }
            }
        }

        protected static void UpdateNetworkWithCreate(Network net, ExtractionDescription description){
            net.AddNode(description.ResultNodeName, description.ResultNodeType,
                        description.AsCreateValue + description.AsOnlyDelta);
            Node n = net[description.ResultNodeName];
            SetupNodeParams(description, n);
        }

        protected static void UpdateNetworkWithDelta(Network net, ExtractionDescription description){
            Node n = net[description.ResultNodeName];
            n.StartWeight += description.AsDeltaValue;
            SetupNodeParams(description, n);
        }

        public void Execute(string text, Network net){
            lock (this){
                Reset();
                Apply(text);
                UpdateNetwork(net);
            }
        }

        public void Reset(){
            foreach (ExtractionDescription description in this){
                description.Reset();
            }
        }

        protected static void SetupNodeParams(ExtractionDescription description, Node n){
            n.Params[PrimaryNodeMeta] = true;
            if (null == n.Params[ExtractorsNodeMeta, null]){
                n.Params[ExtractorsNodeMeta] = new List<ExtractionDescription>();
            }
            n.Params.Get<List<ExtractionDescription>>(ExtractorsNodeMeta).Add(description);
        }


        public void Apply(string text){
            foreach (ExtractionDescription description in this){
                description.Apply(text);
            }
        }

        public IList<ExtractionDescription> GetActiveResultList(){
            return new List<ExtractionDescription>(GetActiveResult());
        }

        public IEnumerable<ExtractionDescription> GetActiveResult(){
            foreach (ExtractionDescription description in this){
                if (description.CanBeCreated || description.CanBeDelted){
                    yield return description;
                }
            }
        }

        public static ExtractionCollection Create(IEnumerable<ExtractionDescription> extractors, bool optimize){
            if (optimize){
                extractors = Optimized(extractors);
            }
            return new ExtractionCollection(extractors);
        }

        public static IEnumerable<ExtractionDescription> Optimized(IEnumerable<ExtractionDescription> extractors){
            var operationIndex = new List<ExtractorOperation>();
            var resultNodes = new Dictionary<string, ExtractionDescription>();
            foreach (ExtractionDescription extractor in extractors){
                if (!resultNodes.ContainsKey(extractor.ResultNodeName)){
                    resultNodes[extractor.ResultNodeName] = extractor;
                }
                foreach (ExtractorOperation operation in extractor.Operations){
                    if (!operationIndex.Contains(operation)){
                        operationIndex.Add(operation);
                    }
                }
            }
            foreach (var node in resultNodes){
                var ops = new List<ExtractorOperation>(node.Value.Operations);
                node.Value.Operations.Clear();
                foreach (ExtractorOperation op in ops){
                    ExtractorOperation top = operationIndex[operationIndex.IndexOf(op)];
                    if (!node.Value.Operations.Contains(top)){
                        node.Value.Operations.Add(op);
                    }
                }
            }

            foreach (ExtractionDescription extractor in extractors){
                ExtractionDescription target = resultNodes[extractor.ResultNodeName];
                foreach (ExtractorOperation op in extractor.Operations){
                    ExtractorOperation top = operationIndex[operationIndex.IndexOf(op)];
                    if (!target.Operations.Contains(top)){
                        target.Operations.Add(op);
                    }
                }
            }

            return resultNodes.Values;
        }
    }
}