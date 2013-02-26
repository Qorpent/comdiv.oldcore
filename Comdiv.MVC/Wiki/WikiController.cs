using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Security;
using Comdiv.Security;
using Comdiv.Wiki;

namespace Comdiv.MVC.Wiki
{
    [Public]
    [ControllerDetails("wiki",Area = "")]
    public class WikiController: BaseController
    {
        public IWikiRepository Repository { get; set; }

        public IWikiRenderService Render { get; set; }
        public void get(string  code) {
            var page = Repository.Get(code);
            page.RenderedContent = Render.Start().Render(page);
            PropertyBag["page"] = page;

        }
        [Role("DOCWRITER")]
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
		[Role("DOCWRITER")]
        public void edit(string code,string  level)
        {
            if(level.noContent()) {
                level = "usr";
            }
            var page =  Repository.Get(code);
            PropertyBag["page"] = page;
            PropertyBag["code"] = code;
            PropertyBag["level"] = null == page ? level : page.Level.ToString();
            PropertyBag["advancedScripts"] = new[]{
                    "yui/utilities/utilities",
                    "yui/container/container",
                    "yui/menu/menu",
                    "yui/button/button",
                    "yui/editor/editor-beta",
                    "editor"};
            PropertyBag["advancedCss"] = new[]{
                    "scripts/yui/fonts/fonts-min",
                    "scripts/yui/container/assets/skins/sam/container",
                    "scripts/yui/button/assets/skins/sam/button",
                    "scripts/yui/editor/assets/skins/sam/editor",
                    "scripts/yui/yahoo-dom-event/yahoo-dom-event",
                    "scripts/yui/menu/assets/skins/sam/menu"};
        }
        public void exists(string  code) {
            RenderText(Repository.Exists(code).ToString());
        }
    }
}
