using System;

namespace Comdiv.Wiki {
    ///<summary>
    ///</summary>
    public interface IWikiPersistenceProvider {
        WikiPage Load(string code);
        bool Exists(string code);
        void Write(WikiPage page);
        void Reload(bool full,string code = null);
        WikiPage[] Search(string  mask);
    }
}