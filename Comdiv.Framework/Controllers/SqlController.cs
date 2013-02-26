using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Booxml;
using Comdiv.Extensions;
using Comdiv.Persistence;

namespace Comdiv.Controllers
{
    /// <summary>
    /// Обеспечивает взаимодействие с SQL БД, активизируется через расширение sql
    /// </summary>
    [Admin]
    public class SqlController:BaseController
    {
        public void index() {
            PropertyBag["connections"] = myapp.sql.GetConnectionNames();
        }
        public void databases(string  system) {
            PropertyBag["databases"] = myapp.sql.GetDatabaseNames(null, system);
        }
        public void connections(string system) {
            PropertyBag["connections"] = myapp.sql.GetConnectionNames();
        }
        public void execute(string query,string database, string system) {
            PropertyBag["result"] = myapp.sql.ExecuteBatch(query, null, database, system);
        }
        public void addconnection(string connection) {
            new BooxmlUtils().EnsureElement("connections.bxl", connection.Split(":"[0])[0], connection.Split(":"[0])[1]);
            RenderText("Соединение создано, требуется перезагрузка!");
        }
        public void removeconnection(string connection) {
            new BooxmlUtils().DeleteElement("connections.bxl", connection.Split(":"[0])[0], "");
            RenderText("Соединение удалено, требуется перезагрузка!");
        }

        public void resources(string assemblyname) {
            var result = new Dictionary<string, string[]>();
            if (assemblyname.hasContent() && assemblyname != "ALL") {
                var assembly =
                    AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                        x => x.GetName().Name.ToLower() == assemblyname.ToLower());
                if (null == assembly) {
                    throw new Exception("no assembly " + assemblyname + " found");
                }
                result[assemblyname] = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".sql")).Select(
                        x => x.find(@"(\w+)\.sql$", 1)).ToArray();
            }else {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    if(assembly.IsDynamic) continue;
                    var resources = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".sql")).Select(
                        x => x.find(@"(\w+)\.sql$", 1)).ToArray();
                    if(resources.Length!=0) {
                        result[assembly.GetName().Name.ToLower()] = resources;
                    }
                }
            }
            PropertyBag["result"] = result;
        }
        public void executeresource(string assemblyname, string resourcename, string system, string  database) {
            var assembly =
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.ToLower() == assemblyname.ToLower());
            if(null==assembly) {
                throw new Exception("no assembly "+assemblyname+" found");
            }
            PropertyBag["result"] = new ResourceScriptRuner().Run(system, database, resourcename, assembly);
            SelectedViewName = "sql/execute";
        }
    }

}
