using System;
using System.IO;
using System.Reflection;
using System.Text;
using Comdiv.Inversion;
using Comdiv.Persistence;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.MAS {
    public class DefaultRepositorySetupProvider:IMasSetupProvider {
        public void Execute(IInversionContainer container, MasConfiguration config) {

            container.ensureService<IConnectionsSource, DefaultConnectionsSource>("default.connectionssource");
            var connections = container.get<IConnectionsSource>();
            if (!connections.Exists(config.System))
            {
                connections.Set(config.System, string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", config.Server, config.Database));
            }

            if (!config.CheckDatabase) return;
            using (var c = connections.Get(config.System).CreateConnection())
            {
                try
                {
                    c.WellOpen();
                }
                catch (Exception ex)
                {
                    throw new MasSetupException("cannot open configured MAS database", ex);
                }
                string schemahash = "";
                try
                {
                    schemahash =
                        c.ExecuteScalar<string>("select comdiv.get_prop_value('comdiv.mas.schema.hashcode')", (IParametersProvider)null);
                }
                catch (Exception ex)
                {
                    throw new MasSetupException("MAS database is not configured properly, cannot get standard comdiv property", ex);
                }

                var schemasqlresourcename =
                    Enumerable.FirstOrDefault<string>(Assembly.GetExecutingAssembly().GetManifestResourceNames(), x => x.Contains("schema.sql"));
                if (null == schemasqlresourcename)
                {
                    throw new MasSetupException("cannot find schema definition resource");
                }
                string script = "";
                using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(schemasqlresourcename)))
                {
                    script = sr.ReadToEnd();
                }
                var actualhash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(script)));
                if (schemahash != actualhash)
                {
                    try
                    {
                        new SqlService().ExecuteBatch(script, null, null, config.System);
                    }
                    catch (Exception ex)
                    {
                        throw new MasSetupException("cannot apply schema to database", ex);
                    }
                }

                try
                {
                    c.ExecuteNonQuery("exec comdiv.set_prop_value 'comdiv.mas.schema.hashcode','" + actualhash + "'", (IParametersProvider)null);
                }
                catch (Exception ex)
                {
                    throw new MasSetupException("cannot setup schema hash in database", ex);
                }
            }
            

        }
    }
}