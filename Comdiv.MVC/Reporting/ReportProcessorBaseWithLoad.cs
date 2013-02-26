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
    public abstract class ReportProcessorBaseWithLoad : ReportProcessorBase{
        public IReportRequestLoader RequestLoader { get; set; }

        protected override IReportRequest loadRequest(string uid, IDictionary<string, object> advancedParameters){
            var loader = RequestLoader;
            if (loader.no()){
                loader = Container.get<IReportFactory>().GetLoader(CallingContext, uid, advancedParameters);
            }
            if (loader.no()){
                throw new ReportLoadException("No loader configured", advancedParameters, uid);
            }
            return loader.Load(uid, advancedParameters, CallingContext);
        }
    }
}