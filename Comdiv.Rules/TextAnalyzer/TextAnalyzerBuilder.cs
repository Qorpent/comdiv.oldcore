using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.TextAnalyzer{
    public class TextAnalyzerBuilder{
        private readonly Dictionary<string, List<ExtractionDescription>> extractors =
            new Dictionary<string, List<ExtractionDescription>>();

        private readonly List<string> modules = new List<string>();

        public void Add(ExtractionDescription extractor){
            Add("default", extractor);
        }

        public void Add(string module, ExtractionDescription extractor){
            if (!modules.Contains(module)){
                modules.Add(module);
            }
            if (!extractors.ContainsKey(module)){
                extractors[module] = new List<ExtractionDescription>();
            }
            extractors[module].Add(extractor);
        }

        public IKnowlegeBase GenerateTextAnalyzer(){
            RuleGroup result = new RuleGroup();
            foreach (string s in modules){
                RuleGroup module = new RuleGroup();
                module.Module = s;
                if (extractors.ContainsKey(s)){
                    ExtractionRule extractor = new ExtractionRule();
                    extractor.Uid = "ta/" + s + "-extractor---" + Guid.NewGuid();
                    extractor.Module = s;
                    foreach (ExtractionDescription d in extractors[s]){
                        extractor.Extractions.Add(d);
                    }
                    extractor.Extractions.Optimize();
                    module.AddRule(extractor);
                }
                result.AddRule(module);
            }
            return result;
        }
    }
}