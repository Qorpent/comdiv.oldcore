using System.IO;
using System.Xml;
using Comdiv.Extensions;


namespace Comdiv.Xml{
    public class IncludeAwareXmlReader{
        private IncludeAwareXmlReader() {}

        public static XmlReader Create(string path){
            return XmlReader.Create(GetCache(path));
        }



        private static string GetCache(string path){
            var cachePath = path + ".cached";
            if (new FileInfo(cachePath).hasNewer("*.xml")){
                var doc = new XmlDocument();
                doc.Load(path);
                doc.ResolveIncludes();
                doc.Save(cachePath);
            }
            return cachePath;
        }
    }


    
}