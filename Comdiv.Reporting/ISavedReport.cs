using System.Collections.Generic;
using System.Security.Principal;
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

namespace Comdiv.Reporting{
    public interface ISavedReport : IEntityDataPattern, IWithUsr{
        bool Shared { get; set; }
        string ReportCode { get; set; }
        IList<ISavedReportParameter> Parameters { get; set; }
        string Role { get; set; }
        bool Authorize(IPrincipal usr);
    }
}