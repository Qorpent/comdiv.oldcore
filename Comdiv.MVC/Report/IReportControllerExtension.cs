#region

using System.IO;
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

#endregion

namespace Comdiv.Web{

    #region

    #endregion

    #region

    #endregion

    public interface IReportControllerExtension{
        string ReportName { get; set; }
        string Type { get; }
        string ViewName { get; set; }
        IReportDefinition Definition { get; set; }
        Controller MyController { get; set; }
    string DefaultViewName { get; set; }
        LongTask Task { get; set; }
        bool PreviewMode { get; set; }
        void ControlPreparation(Controller controller);
        void ControlGeneration(Controller controller);
        void CustomContentPreparatorPrepare(IReportContentPreparator preparator);
        void RenderReport(Controller controller,TextWriter writer = null);

        void
            ControlGeneration(Controller controller, bool render);

        void CheckReportLive();
    }
}