using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;
using Comdiv.Reporting;

namespace Comdiv.Reporting{
    [DefaultImplementation(typeof(ReportCache))]
    public interface IReportCache {
        string Get(IReportDefinition definition);
        void Set(IReportDefinition definition,string content);
        void Clear();
    }
}