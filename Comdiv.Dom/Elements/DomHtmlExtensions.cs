using System.IO;
using System.Linq;
using System.Web.UI;

namespace Comdiv.Dom {
    public static class DomHtmlExtensions
    {
        public static void writeClasses(this HtmlTextWriter writer, INode node)
        {
            writer.WriteAttribute("class", string.Join(" ", node.Classes.ToArray()));
        }

        public static void writeStyles(this HtmlTextWriter writer, INode node)
        {
            writer.WriteAttribute("style", string.Join(";", node.Styles.Select(s => s.Key + ":" + s.Value).ToArray()));
        }

        public static void writeAttributes(this HtmlTextWriter writer, INode node, params string[] excludes)
        {
            foreach (var attr in node.Attributes)
            {
                if (excludes.Contains(attr.Key)) continue;

                writer.WriteAttribute(attr.Key, attr.Value);
            }
        }

        public static string toHtml(this Img img)
        {
            var sw = new StringWriter();
            var writer = new HtmlTextWriter(sw);
            writer.WriteBeginTag("img");
            writer.WriteAttribute("src", img.Href);
            writer.writeClasses(img);
            writer.writeStyles(img);
            writer.writeAttributes(img, "href");
            writer.Write("/>");
            return sw.ToString();
        }
    }
}