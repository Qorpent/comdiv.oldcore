using System;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.Model.Mapping;
using Comdiv.Persistence;
using FluentNHibernate;
using NHibernate.Cfg;
using Environment = NHibernate.Cfg.Environment;

namespace Comdiv.MAS {
    public class MasModel : PersistenceModel,IConfigurationBoundedModel {
        public MasModel() {
            Add(new Automap<Process>());
            Add(new Automap<ProcessMessage>());
            Add(new Automap<ProcessLog>());
            Add(new Automap<Server>());
            Add(new Automap<AppType>());
            Add(new Automap<App>());
            Add(new Automap<TestProvider>());
            Add(new Automap<TestResult>());
        }

        public bool IsFor(Configuration cfg) {
            var name = cfg.GetProperty(Environment.SessionFactoryName);
            using(var connection = myapp.ioc.get<IConnectionsSource>().Get(name).CreateConnection()) {
                try {
                    Console.WriteLine("try connection "+name+" for MAS");
                    connection.WellOpen();
                    var objid = connection.ExecuteScalar<int>("select object_id('mas.process')", (IParametersProvider)null);

                    var result = 0 != objid;
                    Console.WriteLine("connection " + name + "is for MAS: "+result);
                    return result;

                }catch(Exception ex) {
                    Console.WriteLine("connection " + name + "error: " + ex.Message);
                    return false;
                }
            }

        }
    }

}