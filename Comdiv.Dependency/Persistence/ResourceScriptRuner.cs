using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Persistence
{
    public class ResourceScriptRuner
    {
        public SqlBatchResult Run(string system, string database, string scriptname, Assembly assembly = null) {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            var scriptresourcename =
                assembly.GetManifestResourceNames().FirstOrDefault(
                    x => x.Contains(scriptname + ".sql"));
            if(scriptresourcename.noContent()) {
                throw new DataException("cannot find resource "+scriptname+".sql");
            }
            var script = "";
            using (var sr = new StreamReader(assembly.GetManifestResourceStream(scriptresourcename))) {
                script = sr.ReadToEnd();
            }
            var connectionsource = myapp.ioc.get<IConnectionsSource>().Get(system);
            if(null==connectionsource) {
                throw new DataException("cannot find connection " + system );
            }
            return new SqlService().ExecuteBatch(script, null, database, system);
        }
    }
}
