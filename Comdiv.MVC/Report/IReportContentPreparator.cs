using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Dom;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Web{
    public interface IReportContentPreparator{
        IReportControllerExtension Extension { get; set; }
        IDictionary<string, object> Parameters { get; }
        INode BuildContent(IReportControllerExtension extension, Controller controller);
        void SetValue(string name, object value);
    }
}