using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.TextAnalyzer{
    public class TextAnalyzerRule : RuleTemplated{
        protected override void innerInitContext(IRuleContext context){
            if (null == context.Params["ta.defaultdoc", null]){
                foreach (KeyValuePair<string, IDocument> pair in context.Docs.GetAllDocuments()){
                    if (pair.Value is TextAnalyzerDocument){
                        context.Params["ta.defaultdoc"] = pair.Value;
                        break;
                    }
                }
            }
        }

        public TextAnalyzerDocument GetDocumentForContext(IRuleContext context){
            TextAnalyzerDocument result = context.Params["ta.defaultdoc", null] as TextAnalyzerDocument;
            return result;
        }


        protected override bool preTest(IRuleContext context, out bool result){
            if (null == GetDocumentForContext(context)){
                result = false;
                return false;
            }
            return base.preTest(context, out result);
        }
    }
}