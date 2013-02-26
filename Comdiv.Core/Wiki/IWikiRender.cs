using Comdiv.Model.Interfaces;

namespace Comdiv.Wiki {
    public interface IWikiRender:IWithIdx {
        string Render(WikiPage page, string currentcontent);
    }
}