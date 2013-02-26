using Comdiv.Extensions;

namespace Comdiv.Wiki {
    ///<summary>
    ///</summary>
    public static class StandardWikiFormats {
        public const string LinkFinder = @"\[\[(?<code>{0}{1}[\w/\-\d]+)\s*(?<title>[\s\S]*?)\]\]";
        public const string DefaultJsLink = @"<span class='{0}' title='{1}' onclick='{2}'>{3}</span>";
        public static string GetJSLink(string cls,string title, string js, string inner) {
            return string.Format(DefaultJsLink, cls, title, js, inner);
        }
        public static string GetLinkFinder(string folder="") {
            var spliter = folder.hasContent() ? "/" : "";
            return string.Format(LinkFinder, folder, spliter);
        }
    }
}