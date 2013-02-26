using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Xsl;
using Comdiv.Distribution;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Patching;
using Comdiv.Xslt;

namespace Comdiv.MVC.Patching{
    public class XsltTask : IPackageInstallTask{
        public XsltTask(XslCompiledTransform transformation,  string file){
            this.Transformation = transformation;
            this.TargetFile = file;
            this.Name = "xslt";
        }

        public string TargetFile { get; set; }

        public XslCompiledTransform Transformation { get; set; }

        public string Name
        {
            get; set;
        }

        public IPackageInstallResult Do(IPackage package, IFilePathResolver target){
            foreach (var file in TargetFile.split()){
                if (target.Exists(file))
                {
                    var oldcontent = target.Read(file).asXml();
                    var writer = new StringWriter();
                    var args = XsltStandardExtension.PrepareArgs();
                    Transformation.Transform(oldcontent, args, writer);
                    writer.Flush();
                    target.Write(file, writer.ToString());
                }
            }
           
            return new DefaultPackageInstallResult{State = ResultState.OK};
        }
    }
}