using System.Linq;
using System.Xml.Linq;
using Castle.MonoRail.Framework;
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

namespace Comdiv.MVC.Controllers{
    public class ReportOperationLogFilter : OperationLogFilter{
        public ReportOperationLogFilter(){
            writeAfterAction = true;
        }

        protected override void prepareLogEntry(IOperationLog log, IEngineContext context){
            log.ObjectType = "report";
            log.OperationType = "render";

            var def = myapp.conversation.Current.Data["ioc.getdefinition"] as IReportDefinition;
            log.Xml = new XElement("parameters",
                                   from p in def.Parameters
                                   select new XElement(p.Key,
                                                       new XText(p.Value.toStr())
                                       )
                ).ToString();
            var code = "?";
            if (def != null){
                code = def.Code;
            }

            log.ObjectCode = code;
        }
    }
}