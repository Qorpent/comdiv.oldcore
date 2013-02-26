using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Extensions;
using Qorpent.Bxl;
using FilePathResolverExtensions = Comdiv.IO.FilePathResolverExtensions;

namespace Comdiv.Booxml {
    public class BooxmlUtils {
        public void EnsureElement(string filename, string elementname, string id, params object [] allcontent) {
            var content = FilePathResolverExtensions.Read(myapp.files, filename);
            if(content.noContent()) {
                content = "#empty";
            }
            var x = new BxlParser().Parse(content);
            var element = x.XPathSelectElement(string.Format("./{0}[@id='{1}']", elementname, id));

            if(null==element) {
                element = new XElement(elementname, new XAttribute("id",id));
                x.Add(element);
            } else {
                if(allcontent.no()) {
                    return;
                }
            }
            if(allcontent.yes()) {
                foreach (var node in allcontent) {
                    element.Add(node);
                }
            }
            var newcontent = new BooxmlGenerator().Generate(x);
            FilePathResolverExtensions.Write(myapp.files, filename,newcontent);
        }

        public void DeleteElement(string filename, string elementname, string id) {
            var content = FilePathResolverExtensions.Read(myapp.files, filename);
            if (content.noContent())
            {
                content = "#empty";
            }
            var x = new BxlParser().Parse(content);
            var xpath = "./" + elementname;
            if(id.hasContent()) {
                xpath += "[@id='" + id + "']";
            }
            var elements = x.XPathSelectElements(xpath);
            if (elements.no()) return;
            foreach (var element in elements) {
                element.Remove();    
            }
            
            var newcontent = new BooxmlGenerator().Generate(x);
            FilePathResolverExtensions.Write(myapp.files, filename, newcontent);
        }
    }
}