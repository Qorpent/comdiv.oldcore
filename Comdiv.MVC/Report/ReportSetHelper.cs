using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
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
using Comdiv.Xml;

namespace Comdiv.Web{
    public class ReportSetHelper{
        public static IEnumerable<IReportDefinition> GetForClass(string cls){
            return GetAll().Where(rd => cls == rd.Controller);
        }

        public static IEnumerable<IReportDefinition> GetAll(){
            var a = getXmlDefinitions();
            foreach (var r in a){
                
                var type = r.attr("type");
                var _type = typeof (ReportDefinitionBase);
                if (type.hasContent()){
                    _type = type.toType();
                }
                var xmlreader = _type.create<IXmlReadable>();
                xmlreader.ReadFromXml(r);
                yield return (IReportDefinition) xmlreader;
            }
        }

        protected static IEnumerable<XElement> getXmlDefinitions(){
            var file = getXmlDefinitionsFileName();
            if (null == file)
            {
                return new XElement[]{};
            }
            return XElement.Load(file).XPathSelectElements("./report");
        }

        
        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(ReportSetHelper)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        protected static string getXmlDefinitionsFileName(){
            return Container.get<IFilePathResolver>().Resolve("report-defs.xml");
        }
    }
}