using System.Collections.Generic;
using Castle.MonoRail.Framework;
using Comdiv.Authorization;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Security;
using Comdiv.Wiki;

namespace Comdiv.Controllers
{
    [Public]
    [ControllerDetails("wiki",Area = "")]
    public class WikiController: BaseController
    {
        public IWikiRepository Repository { get; set; }

        public IWikiRenderService Render { get; set; }
        public void get(string  code, string mask) {
            IList<WikiPage> pages = new List<WikiPage>();
            if (mask.noContent()) {
                var page = Repository.Get(code);
                page.RenderedContent = Render.Start().Render(page);
                pages.Add(page);
            }else {
                var pages_ = Repository.Search(mask);
                foreach (var page in pages_) {
                    page.RenderedContent = Render.Start().Render(page);
                    pages.Add(page);
                }
            }
            PropertyBag["pages"] = pages;

        }
        [Role("WIKIAUTHOR")]
        public void set(string code, string  content, string  level) {
            var page = Repository.Get(code);
            if(null==page) {
                page = new WikiPage {Code = code};
            }
            FileLevel _level;
            FileLevel.TryParse(level,out _level);
            page.Level = _level;
            page.Content = content;
            Repository.Save(page);
            RenderText("OK");
        }
        [Role("WIKIAUTHOR")]
        public void edit(string code,string  level)
        {
            if(level.noContent()) {
                level = "usr";
            }
            var page =  Repository.Get(code);
            PropertyBag["page"] = page;
            PropertyBag["code"] = code;
            PropertyBag["level"] = null == page ? level : page.Level.ToString();           
        }
        public void exists(string  code) {
            RenderText(Repository.Exists(code).ToString());
        }
    }
}
