using System.Collections.Generic;


namespace Comdiv.Rules.Context{
    public interface IDocumentProvider{
        IDocument this[string name] { get; set; }
        IRuleContext ContainingContext { get; set; }
        IDictionary<string, IDocument> GetAllDocuments();
        bool Exists(string name);
    }
}