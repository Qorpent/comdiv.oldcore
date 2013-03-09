using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Xml;

namespace Comdiv.Reporting{
    /// <summary>
    /// Static report definition - contains parameters which are stable for several requests, in 
    /// RGP it means as DEFAULT behaviour, intended ConvertTo be storable
    /// Статическое определение отчета, содердит стабильные параметры по умлочанию, в ЛГО выступает как поведение
    /// по умолчанию, предполагается, что имеет тенденцию быть хранимым
    /// </summary>
    public interface IReportDefinition : IXmlReadable, IXmlWriteable,IWithName,IWithCode,IWithComment, IWithRole{
        IList<IReportDefinition> Sources { get; }
        string Controller { get; set; }
        string Area { get; set; }
        string AdvancedParameters { get; set; }
        Dictionary<string, object> Parameters { get; }
        ParametersCollection TemplateParameters { get; }
        string PageTitle { get; }
        ReportWidgetCollection Widgets { get; }
        object Thema { get; set; }
        IDictionary<string, string> Extensions { get; }
        LongTask Task { get; set; }
        bool PreviewMode { get; set; }
        object ControllerInstance { get; set; }
        IReportDefinition LoadParameters();
        IReportDefinition LoadParameters(NameValueCollection collection);
        IReportDefinition Clone();
        void CleanupParameters(IPrincipal principal);
        void CheckReportLive();
    }
}