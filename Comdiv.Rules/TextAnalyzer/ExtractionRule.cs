using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.TextAnalyzer{
    public class ExtractionRule : TextAnalyzerRule{
        public ExtractionRule(){
            SetupCountHints(1, 1);
        }

        public ExtractionRule(ExtractionDescription desc, params ExtractionDescription[] extractors) : this(){
            Extractions.Add(desc);
            Extractions.AddRange(extractors);
            Extractions.Optimize();
        }


        public ExtractionCollection Extractions{
            get{
                if (null == Params["ta.extractions", null]){
                    Params["ta.extractions"] = new ExtractionCollection();
                }
                return Params.Get<ExtractionCollection>("ta.extractions");
            }
        }

        protected override bool preTest(IRuleContext context, out bool result){
            if (0 == Extractions.Count){
                result = false;
                return false;
            }
            return base.preTest(context, out result);
        }

        protected override bool innerTest(IRuleContext context){
            Extractions.Reset();
            Extractions.Apply(GetDocumentForContext(context).Text);
            var result = Extractions.GetActiveResultList();
            if (0 == result.Count){
                return false;
            }
            context.RuleData[this, "ta.activatedExtractions"] = result;
            return true;
        }

        protected override void innerExecute(IRuleContext context){
            IList<ExtractionDescription> result = context.RuleData.Get<IList<ExtractionDescription>>(this,
                                                                                                     "ta.activatedExtractions");
            ExtractionCollection.DoUpdateNetwork(GetDocumentForContext(context).SemanticNetwork, result);
        }
    }
}