using Comdiv.Extensions;

namespace Comdiv.Wiki {
    public class WikiRenderExecutor {
        private IWikiRender[] renders;

        public WikiRenderExecutor(IWikiRender[] renders) {
            this.renders = renders;
        }

        public string Render(WikiPage page) {
           
            var current = page.Content;
            foreach (var render in renders) {
                current = render.Render(page, current);
            }
            return current;
        }
    }
}