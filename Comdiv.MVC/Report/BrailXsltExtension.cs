using System.Collections.Generic;
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
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Web{
    internal class BrailXsltExtension{
        public Controller Controller { get; set; }
        public IReportControllerExtension ReportControllerExtension { get; set; }

        public string View(string viewname){
            return View(viewname, null);
        }

        public string View(string viewname, string parameterString){
            var parameters = new Dictionary<string, object>();
            foreach (var key in Controller.PropertyBag.Keys){
                parameters[key.ToString()] = Controller.PropertyBag[key];
            }
            parameters["pageTitle"] = "Отчет";
            if (null != ReportControllerExtension.Definition){
                foreach (var pair in ReportControllerExtension.Definition.Parameters){
                    parameters[pair.Key] = pair.Value;
                }
            }
            if (!parameters.ContainsKey("definition")){
                parameters["definition"] = ReportControllerExtension.Definition;
            }

            foreach (var parameter in parameterString.split()){
                var pdef = parameter.Split(':');
                parameters[pdef[0]] = pdef[1];
            }
            var writer = new StringWriter();
            var eng = Web.ReportControllerExtension.GetBrail(Controller);
            string result;

            eng.Process(viewname, null, writer, parameters);
            result = writer.ToString();
            return result;
        }
    }
}