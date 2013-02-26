using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Reporting.Security{
    public class ReportTokenProvider : XmlBasedTokenProvider<IReportDefinition>{
        public ReportTokenProvider(){
            FileName = "report.acl.token.config";
            DefaultPrefix = "report";
        }

        public override string suffix(IReportDefinition o){
            var aclparameters = o.TemplateParameters.AllParameters(). Where(x => x.Acl).OrderBy(x => x.AclIdx);
            var result = "";
            foreach (var parameter in aclparameters){
                var name = parameter.AclPrefix.hasContent() ? parameter.AclPrefix : parameter.Target;
                var value = o.Parameters[parameter.Target];
                if (null == value){
                    value = "null";
                }
                result += name + "_" + value + "/";
            }
            return result;
        }
    }
}