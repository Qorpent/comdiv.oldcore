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
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Report{
    public class MultipleProdcution : ProductionBase, IMultipleReportProduction{
        internal IList<IReportProduction> ProductionList = new List<IReportProduction>();

        #region IMultipleReportProduction Members

        public IEnumerable<IReportProduction> Productions{
            get { return ProductionList; }
        }

        public override byte[] GetBinaryPresentation(){
            throw new ReportException("Not supported");
        }

        public override string GetStringPresentation(){
            throw new ReportException("Not supported");
        }

        #endregion
    }
}