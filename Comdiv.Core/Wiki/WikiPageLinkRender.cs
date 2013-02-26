using System;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Security;
namespace Comdiv.Wiki {

   

    public class WikiPageLinkRender : IWikiRender, IWikiAwared {
        public WikiPageLinkRender() {
            Idx = 1000;
            Folder = "";
            CreateRole = "WIKIAUTHOR";
            GoClass = "wiki-link";
            CreateClass = "wiki-createlink";
            NotExistedClass = "wiki-notexisted";
            GoJs = "wiki.open(\"{0}\");";
            CreateJs = "wiki.edit(\"{0}\");";
            NotExistedJs = "wiki.notexisted(\"{0}\");";
        }

        public string Folder { get; set; }
        public string CreateRole { get; set; }
        public string GoClass { get; set; }
        public string CreateClass { get; set; }
        public string NotExistedClass { get; set; }
        public string GoJs { get; set; }
        public string CreateJs { get; set; }
        public string NotExistedJs { get; set; }
        #region IWikiAwared Members

        public IWikiRenderService RenderService { get; set; }
        public IWikiRepository Repository { get; set; }


        #endregion

        #region IWikiRender Members

        public int Idx { get; set; }

        public string Render(WikiPage page, string currentcontent) {
            string finder = StandardWikiFormats.GetLinkFinder(Folder);
            currentcontent = currentcontent.replace(finder,
                                                    m =>
                                                        {
                                                            string code = m.Groups["code"].Value;
                                                            string title = m.Groups["title"].Value;
                                                            bool existed = Repository.Exists(code);
                                                            return formatLink(code, title, existed);
                                                        }
                );
            return currentcontent;
        }

        #endregion

        protected virtual string formatLink(string code, string title, bool existed) {
            var spantitle = getTitle(code, title, existed);
            var spantext = getText(code, title, existed);
            if(existed) {
                return StandardWikiFormats.GetJSLink(GoClass, spantitle, string.Format(GoJs,code), spantext);
            }else {
                if(CreateRole.noContent() ||  myapp.roles.IsInRole(CreateRole)) {
                    return StandardWikiFormats.GetJSLink(CreateClass, spantitle, string.Format(CreateJs, code), spantext);    
                }else {
                    return StandardWikiFormats.GetJSLink(NotExistedClass, spantitle, string.Format(NotExistedJs, code), spantext);
                }
            }
        }

        protected virtual string getText(string code, string title, bool existed) {
            return title;
        }

        protected virtual string getTitle(string code, string title, bool existed) {
            if(existed) {
                return "Открыть страницу: " + title;
            }else {
                if (CreateRole.noContent() || myapp.roles.IsInRole(CreateRole))
                {
                    return "Создать страницу: " + title;
                }
                else
                {
                    return "Документ отсутствует: " + title;
                }
            }
        }
    }
}