using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Comdiv.MVC.Report{
    public interface IReportFactory{
        Type DefaultRequestType { get; set; }
        Type DefaultDefinitionType { get; set; }
        IList<Func<IMvcContext, string, IDictionary<string, object>, IReportRequestLoader>> LoaderFunctions { get; }
        IList<Func<IMvcContext, IDictionary<string, object>, IReportRequestInitiator>> InitFunctions { get; }
        IReportRequestLoader GetDefaultRequestLoader();
        IReportRequestInitiator GetDefaultRequestInitiator();
        IReportRequest CreateEmptyRequest();

        IReportRequestLoader GetLoader(IMvcContext context, string uid,
                                       IDictionary<string, object> parameters);

        IReportDefinition CreateEmptyDefiniton();
        IReportRequest LoadRequest(Controller controller);
        IReportRequest LoadRequest(Controller controller, Type definitionType);
        IList<Entity> GetAvailableReportTemplates();
        IReportDefinition LoadDefinition(string code, Type definitionType);
        IReportRequest InitRequest(Controller controller, Dictionary<string, object> parameters);
    }
}