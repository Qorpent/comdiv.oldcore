using System.Collections.Generic;
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

namespace Comdiv.MVC.Report{
    public class ReportRequest : IReportRequest{
        private IDictionary<string, object> parameters = new Dictionary<string, object>();

        #region IReportRequest Members

        public IReportRequestIdentity RequestId { get; set; }

        public IDictionary<string, object> Parameters{
            get { return parameters; }
            set { parameters = value; }
        }

        public IReportDefinition ReportDefinition { get; set; }

        public IReportProductionFormat ProductionFormat { get; set; }

        public IReportOutputFormat OutputFormat { get; set; }

        public IMvcContext CallingContext { get; set; }

        #endregion

        public override string ToString(){
            if (ReportDefinition != null){
                return string.Format("REPORT: {0}/{1}/{2} :: ", ReportDefinition.Area ?? "default",
                                     ReportDefinition.Controller ?? "default",
                                     ReportDefinition.Code ?? "default");
            }
            return "REPORT";
        }
    }
}