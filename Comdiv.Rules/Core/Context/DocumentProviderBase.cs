using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Support{
    public class DocumentProviderBase : IDocumentProvider{
        private IDictionary<string, IDocument> data;

        public DocumentProviderBase(IRuleContext containingContext){
            ContainingContext = containingContext;
        }

        public DocumentProviderBase() {}

        protected IDictionary<string, IDocument> Data{
            get { return data ?? (data = new Dictionary<string, IDocument>()); }
        }

        #region IDocumentProvider Members

        public IRuleContext ContainingContext { get; set; }


        public IDictionary<string, IDocument> GetAllDocuments(){
            return Data;
        }

        public IDocument this[string name]{
            get { return Data[name]; }
            set { Data[name] = value; }
        }

        public bool Exists(string name){
            return Data.ContainsKey(name);
        }

        #endregion
    }
}