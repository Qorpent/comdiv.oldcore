namespace Comdiv.Wiki {
    public interface IWikiRepository {
        WikiPage Get(string code);
        bool Exists(string code);
        void Save(WikiPage page);
        WikiPage Refresh(WikiPage page);
        WikiPage[] Search(string mask);
    }
}