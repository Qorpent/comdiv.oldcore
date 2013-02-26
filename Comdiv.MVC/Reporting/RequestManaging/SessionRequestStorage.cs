using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Caching;
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
using dict = System.Collections.Generic.Dictionary<string, Comdiv.MVC.Report.IReportRequest>;

namespace Comdiv.MVC.Report{
    public class CacheRequestStorage : ReportRequestLoaderBase, IReportRequestInitiator{
        #region IReportRequestInitiator Members

        public IReportRequest InitRequest(IMvcContext callingContext){
            return InitRequest(callingContext, null);
        }

        public IReportRequest InitRequest(IMvcContext callingContext, IDictionary<string, object> parameters){
            return CreateRequest(callingContext, parameters);
        }

        #endregion

        protected override IReportRequest innerLoad(string uid, IDictionary<string, object> parameters,
                                                    IMvcContext callingContext){
            return Container.get<IApplicationCache>().Get<IReportRequest>(uid);
        }


        protected IReportRequest CreateRequest(IMvcContext context, IDictionary<string, object> parameters){
            var newrequest = Container.get<IReportFactory>().CreateEmptyRequest();
            var key = Container.get<IApplicationCache>().Store(newrequest);
            newrequest.RequestId = new ReportRequestIdentity{Uid = key};
            if (parameters.yes()){
                newrequest.Parameters = new Dictionary<string, object>(parameters);
            }
            return newrequest;
        }
    }
}