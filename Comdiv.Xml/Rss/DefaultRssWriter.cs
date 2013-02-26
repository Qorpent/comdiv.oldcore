using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Comdiv.Extensions;
using Comdiv.Rss;

namespace Comdiv.Rss{
    public class DefaultRssWriter : IRssWriter
    {
        public void Write(string fileName, IRssSource src)
        {
            using (var writer = XmlWriter.Create(fileName, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                Write(src, writer);
                writer.Flush();
            }
        }

        private void Write(IRssSource src, XmlWriter writer)
        {
            if (src.IsRetranslator)
            {
                writer.WriteRaw(src.GetRaw());
            }
            else{
                writeEmbeded(src, writer);
            }
        }

        private void writeEmbeded(IRssSource src, XmlWriter writer) {
            new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(
                    "rss", new XAttribute("version", "2.0"),
                    new XElement("channel",
                                 new XElement("title", src.Channel.Title),
                                 new XElement("link", src.Channel.Link),
                                 new XElement("description", src.Channel.Description),
                                 new XElement("pubDate", src.Channel.PubDate.toInternetDateString()),
                                 new XElement("lastBuildDate",src.Channel.LastBuildDate.toInternetDateString()),
                                 new XElement("generator", src.Channel.Generator),
                                 from item in src.Items
                                 select
                                     new XElement("item",
                                                  new XElement("title", item.Title),
                                                  new XElement("link", item.Link),
                                                  new XElement("description", item.Description),
                                                  from pair in item.Categories
                                                  select new XElement("category", new XAttribute("domain", pair.Key), pair.Value),
                                                  new XElement("guid", item.Guid),
                                                  new XElement("pubDate", item.PubDate.toInternetDateString()),
                                                  new XElement("source",item.Source),
                                                  new XElement("date",item.PubDate.ToString("dd.MM.yyyy"))
                                                  
                                     )
                                     
                        )
                    )
                ).WriteTo(writer);
        }

        public string ToString(IRssSource source){
            using (var sw = new StringWriter()){
                using (var w  = XmlWriter.Create(sw,new XmlWriterSettings{Encoding=Encoding.UTF8,Indent=true,OmitXmlDeclaration = true
                })){
                    this.Write(source,w);
                    w.Flush();
                }
                return sw.ToString();
            }
        }
    }
}