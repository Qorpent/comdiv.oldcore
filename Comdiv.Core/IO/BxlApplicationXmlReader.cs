using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Xml.Smart;
using Qorpent.Bxl;

namespace Comdiv.IO {
    /// <summary>
    /// Application Xml reader with BXL ability
    /// </summary>
    public class BxlApplicationXmlReader:ApplicationXmlReader {
        private string directbxl;
        public BxlApplicationXmlReader() {
            Except = "/extensionslib/";
        }
        public BxlApplicationXmlReader(string bxl){
            this.directbxl = bxl;
        }
		BxlParser bxl = new BxlParser();

        public bool TotalSearch { get; set; }

        public string Except { get; set; }


        protected override void internalReadSources(Dictionary<string, System.Xml.Linq.XElement> result, string path) {
            if(directbxl!=null){
                result["directbxl"] = bxl.Parse(directbxl);
                return;
            }
            
            var bxlpath = path.replace(@"\.\w+$", ".bxl");

            if(TotalSearch) {
                readByTotalSearch(result, bxlpath);
                return;
            }

            var bxls = myapp.files.ResolveAll("", bxlpath);
            foreach (var b in bxls){
                var element =new SmartXml( bxl.Parse(File.ReadAllText(b))).Process().Element;
                foreach (var e in element.Elements()) {
                    e.Add(new XAttribute("_srcbxlfile",b));
                }
                result[b] = element;
            }
        }

        private void readByTotalSearch(Dictionary<string, XElement> result, string bxlpath) {
            var root = myapp.files.Resolve("~/", true);
            var files = Directory.EnumerateFiles(root, bxlpath, SearchOption.AllDirectories)
                .Select(x=>x.ToLower().Replace("\\","/"))
                .Where(x => Except.noContent() || !x.like(Except))
                .Where(x=>!x.like("/profile/") || x.like("/profile/"+myapp.usrName.ToLower().Replace("\\","_")+"/"))
                .OrderByDescending(x=>x,new LevelPathComparer())
            ;
            foreach (var file in files) {
                var element = bxl.Parse(File.ReadAllText(file));
                 foreach (var e in element.Elements()) {
                    e.Add(new XAttribute("_srcbxlfile",file));
                }
                result[file] = element;
            }

        }

 
    }
}
