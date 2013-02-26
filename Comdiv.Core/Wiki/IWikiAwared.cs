namespace Comdiv.Wiki {
    public interface IWikiAwared {
        IWikiRenderService RenderService { get; set; }
        IWikiRepository Repository { get; set; }
    }
}