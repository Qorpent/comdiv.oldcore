using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Comdiv.Distribution;

using Comdiv.IO;

namespace Comdiv.MVC.Patching{
    public class XsltPatch : MvcPatch{
        public string XsltFile { get; set; }
        public string TargetFile { get; set; }
        public XslCompiledTransform Transformation { 
            get{
                var result = new XslCompiledTransform();
                var xslt = FileSystem.Read(XsltFile + ".base").asXml();
                result.Load(xslt, XsltSettings.TrustedXslt, new FileSystemXmlResolver(FileSystem,FileSystem.ReadBinary(XsltFile+".base")));
                return result;
            }
        }
        public XsltPatch(){
            AutoLoad = true;
            ReInstallable = true;
        }
        public override IList<IPackageInstallTask> Tasks
        {
            get
            {
                if (base.Tasks.Count == 0)
                {
                    base.Tasks.Add(new XsltTask(Transformation,TargetFile));
                }
                return base.Tasks;
            }
        }
    }
}