using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Comdiv.Rules.SemanticNet;

namespace Comdiv.Rules.TextAnalyzer{
    public class TextAnalyzerDocument : DocumentBase{
        public TextAnalyzerDocument() {}

        public TextAnalyzerDocument(string text, IXPathNavigable advancedXmlData){
            Text = text;
            AdvancedXmlData = advancedXmlData;
        }

        public Network SemanticNetwork{
            get{
                var result = Params.Get<Network>("network", null);
                if (null == result){
                    result = new Network();
                    Params["network"] = result;
                }
                return result;
            }
        }

        public string Text{
            get { return Params.Get<string>("text"); }
            protected set { Params["text"] = value; }
        }

        public IXPathNavigable AdvancedXmlData{
            get { return Params.Get<IXPathNavigable>("advancedXmlData"); }
            protected set { Params["advancedXmlData"] = value; }
        }
    }
}