using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Inversion;
using Comdiv.Persistence;
using FluentNHibernate;
using NHibernate.Mapping;

namespace Comdiv.Controllers
{
[Admin]
    public class SysInfoController:BaseController
    {
        public void index() {
        
        }
        public void controllers() {
            var controllers = myapp.ioc.all<IController>();
            PropertyBag["result"] = controllers.ToArray();
        }

        public void viewroots() {
            var result = new List<string>();
            result.Add(MonoRailConfiguration.GetConfig().ViewEngineConfig.ViewPathRoot);
           
            foreach (var path in MonoRailConfiguration.GetConfig().ViewEngineConfig.PathSources) {
                result.Add(path);
            }
            PropertyBag["result"] = result;
        }

        public void widgets() {
            PropertyBag["result"] = myapp.widgets.GetAllWidgets().OrderBy(x => x.Position + "_" + (x.Idx + 10000)).ToArray();
        }

        public void dlls() {
            PropertyBag["result"] = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.GetName().Name.StartsWith("System")
                && !x.GetName().Name.StartsWith("Microsoft")
                && !x.GetName().Name.StartsWith("mscorlib")
                ).ToArray();
        }

        public void models() {
            PropertyBag["result"] = myapp.ioc.all<PersistenceModel>().ToArray();
        }

        public void hncfgs() {
            PropertyBag["result"] = myapp.ioc.get<IConfigurationProvider>().GetIds().ToArray();
        }

        public void hnclasses(string  cfg) {
             
            PropertyBag["result"] = myapp.ioc.get<IConfigurationProvider>().Get(cfg).ClassMappings.ToArray();
        }
    }
}
