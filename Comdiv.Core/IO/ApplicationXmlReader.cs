using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Xml.Smart;

namespace Comdiv.IO {
    public interface IApplicationXmlReader{
        XElement Read(string path);
    }
    public class ApplicationXmlReader:IApplicationXmlReader {
        private XElement directxml;
        public ApplicationXmlReader(){
            
        }
        public ApplicationXmlReader(string  xml){
            if(null!=xml){
                directxml = XElement.Parse(xml);
            }
        }

        public ApplicationXmlReader(XElement xml){
            this.directxml = xml;
        }

        

        public XElement Read(string path){
            lock (this){
                var dict = readSources(path);
                var result = new XElement("root");
                foreach (var e in dict.Keys.OrderByDescending(x=>x,new LevelPathComparer())){
                    result.Add(dict[e].Elements());
                }
                new SmartXml(result).Process();
                return result;
            }
        }

        private IDictionary<string,XElement> readSources(string path){
            var result = new Dictionary<string, XElement>();
            internalReadSources(result, path);
            return result;
        }

        protected  virtual void internalReadSources(Dictionary<string, XElement> result, string path){
            if(directxml!=null){
                result["directxml"] = directxml;
                return;
            }
            var xmls = myapp.files.ResolveAll((string) "", path, (bool) true, (Action<string>) null);
            foreach (var xml in xmls){
                result[xml] = XElement.Load(xml);
            }
        }
    }
}
