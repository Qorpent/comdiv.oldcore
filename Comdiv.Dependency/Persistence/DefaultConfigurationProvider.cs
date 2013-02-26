// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;
using Comdiv.Model.Mapping;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Bytecode;
using NHibernate.Cfg;
using NHibernate.Context;
using NHENV=NHibernate.Cfg.Environment;

namespace Comdiv.Persistence{
    public class DefaultConfigurationProvider :
        DefaultConfigurationProvider<AutomativeCurrentSessionContext, DefaultProxyFactoryFactory> {}

    public class DefaultConfigurationProvider<TCurrentContext, TProxyFactoryFactory>
        : IConfigurationProvider, IWithContainer
        where TCurrentContext : ICurrentSessionContext
        where TProxyFactoryFactory : IProxyFactoryFactory{
        private readonly IDictionary<string, Configuration> _configurations = new Dictionary<string, Configuration>();
        private readonly IList<PersistenceModel> _manualSettedModels = new List<PersistenceModel>();
        private readonly ILog log = logger.get("comdiv.persistence.configurationprovider");
        private bool _configured;
        

        public DefaultConfigurationProvider(){
           // ConnectionsSource = new DefaultConnectionsSource();
            UseDefaultContainerIfNoModelsProvided = true;
        }

        private string _connection;
        public string Connection {
            get {
                /*if(null==_connection) {
                    var src = ConnectionsSource.GetConnections().FirstOrDefault();
                    if(null==src) {
                        _connection = "";
                    }else {
                    _connection = src.ConnectionString;
                        }
                }*/
                return _connection;
            }
            set { _connection = value; }
        }

        public bool UseDefaultContainerIfNoModelsProvided { get; set; }

        public IList<PersistenceModel> ManualSettedModels{
            get { return _manualSettedModels; }
        }

        private IConnectionsSource _connectionsSource;
        public IConnectionsSource ConnectionsSource {
            get {
                if(_connectionsSource==null) {
                    _connectionsSource = Container.get<IConnectionsSource>() ?? new DefaultConnectionsSource();
                }
                return _connectionsSource;
            }
            set { _connectionsSource = value; }
        }

        public IDictionary<string, Configuration> Configurations{
            get{
                Configure();
                return _configurations;
            }
        }

        public string FactoryNamePrefix { get; set; }

        #region IConfigurationProvider Members

        public IEnumerable<string> GetIds(){
            Configure();
            return Configurations.Keys.ToArray();
        }

        public Configuration Get(string id){
            
                if (id == null) {
                    return Configurations.FirstOrDefault().Value;
                }
                return Configurations[id];
            
        }

        public event EventHandler<ConfiguringEventArgs> BeforeConfigure;
        public event EventHandler<ConfigurationEventArgs> AfterConfigure;

        #endregion

        #region IWithContainer Members
        private IInversionContainer _container;
        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        #endregion

        public IEnumerable<PersistenceModel> GetModels(){
            if (ManualSettedModels.Count != 0){
                return ManualSettedModels;
            }
            if (!UseDefaultContainerIfNoModelsProvided){
                return new PersistenceModel[]{};
            }
            return Container.all<PersistenceModel>();
        }

        private void NativeConfiguration() {
            var configurations = getConfigs();
            log.Debug("Configure new session factories from the fluent configuration.");
            foreach (var fluent in configurations) {
                if (null == fluent) {
                    continue;
                }
                bool configured;
                var configuration = new Configuration();
                DoBeforeConfigure(configuration, out configured);
                if (!configured) {
                    configuration = fluent.BuildConfiguration();

                    foreach (var model in GetModels()) {
                        if (model is IConfigurationBoundedModel) {
                            if (!((IConfigurationBoundedModel) model).IsFor(configuration)) {
                                continue;
                            }
                        }
                        model.Configure(configuration);
                    }


                }
                DoAfterConfigure(configuration);

                _configurations[configuration.GetProperty(NHENV.SessionFactoryName)] = configuration;
                storeConfigurations();
            }
        }

        public void Configure(){
            if (!_configured){
                lock (this) {
                    if(_configured) return;
                    if (!LoadSavedConfig()) {
                        NativeConfiguration();
                    }
                }
                    _configured = true;
                }
            
            
        }

        private bool LoadSavedConfig() {
#if USESAVEDNH
            var filename = myapp.files.Resolve("~/tmp/hibernate.bin", false);
            if (!File.Exists(filename)) return false;
            try {
                using (var file = File.Open(filename, FileMode.Open)) {
                    var bf = new BinaryFormatter();
                    var __configurations = bf.Deserialize(file) as IDictionary<string, Configuration>;
                    foreach (var configuration in __configurations) {
                        _configurations[configuration.Key] = configuration.Value;
                    }
                    return true;
                }
            }catch(Exception ex) {
                return false;
            }
#else
            return false;
#endif
        }

        private void storeConfigurations() {
#if USESAVEDNH
            var filename = myapp.files.Resolve("~/tmp/hibernate.bin", false);
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using (var file = File.Open(filename, FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(file, _configurations);
            }
#endif
        }


        protected FluentConfiguration[] getConfigs(){
            log.debug(() => "start get configurations");
            if (Connection.hasContent()){
                log.debug(() => "by given connection " + Connection);
                var fluentConfiguration = getConfig(Connection, Connection);
                return new[]{fluentConfiguration};
            }
            IList<FluentConfiguration> result = new List<FluentConfiguration>();
            var connections = ConnectionsSource.GetConnections();
            if (connections.Count() == 0){
                log.info(() => "no connections configured");
            }

            foreach (var nc in connections){
                log.debug(() => ("by named connection " + (nc.Name ?? "")));
                result.Add(getConfig(nc.Name, nc.ConnectionString ?? ""));
            }
            return result.ToArray();
        }

        private PersistenceConfiguration<X, T> defaultConfiguration<X, T>(PersistenceConfiguration<X, T> configuration,
                                                                          string connectionName, string connectionString)
            where X : PersistenceConfiguration<X, T> where T : ConnectionStringBuilder, new() {
            var proxyFactory = typeof (TProxyFactoryFactory).AssemblyQualifiedName;
            var sessionContextClass = typeof (TCurrentContext).AssemblyQualifiedName;
            connectionName = connectionName.hasContent() ? connectionName : Guid.NewGuid().ToString();
            configuration.ConnectionString(c => c.Is(connectionString))
				 .ProxyFactory(proxyFactory)
                .UseOuterJoin()
                .DoNot.ShowSql()
                .QuerySubstitutions("true 1, false 0, yes 'Y', no 'N'")
               
                .UseReflectionOptimizer()
                .AdoNetBatchSize(100)
                .Raw("prepare_sql","true")
                .Raw("__connection", connectionString)
                .Raw("__connectionname", connectionName)
                .Raw("command_timeout", "100")
                .Raw("session_factory_name", (FactoryNamePrefix ?? "") + connectionName)
                .Raw("current_session_context_class", sessionContextClass)
                .Raw("prepare_sql", "false")
                .Raw("hbm2ddl.keywords", "none")
                ;

            return configuration;
        }

        private FluentConfiguration getConfig(string connectionName, string connectionString){
            if(null==connectionString && null==connectionName){
                return null;
            }
            if (connectionString.Length <= 15){
                var connectionStrings = ConnectionsSource.GetConnections().ToArray();
                if (0 != connectionStrings.Length){
                    var cs_ = connectionStrings.FirstOrDefault(x => x.Name == connectionName);
                    if (null != cs_){
                        connectionString = cs_.ConnectionString ?? "";
                    }
                }
            }
            if(string.IsNullOrWhiteSpace(connectionString)) {
                return null;
            }

            IPersistenceConfigurer db;

            if (connectionName == TestEnvironment.TestSqlLiteFactoryName){
                db = prepareSqlLite();
            }
            else if (connectionName == TestEnvironment.TestPostgresSqlFactoryName){
                db = prepareTestPG();
            }
            else if (connectionString.ToLower().Contains("postgres")){
                db = defaultConfiguration(PostgreSQLConfiguration.Standard, connectionName, connectionString)
                    ;
            }
            else if (connectionString.ToLower().Contains("oracle")){
                db = defaultConfiguration(OracleClientConfiguration.Oracle9, connectionName, connectionString);
            }
            else{
                db = defaultConfiguration(MsSqlConfiguration.MsSql2005, connectionName, connectionString);
            }
            if(null==db) {
                return null;
            }
            return Fluently.Configure().Database(db);
        }

        private IPersistenceConfigurer prepareTestPG(){
            string connectionString;
            IPersistenceConfigurer db;
            connectionString = TestEnvironment.TestPostgresSqlConnectionString;
            db = defaultConfiguration(PostgreSQLConfiguration.Standard, TestEnvironment.TestPostgresSqlFactoryName,
                                      connectionString);
            return db;
        }

        private IPersistenceConfigurer prepareSqlLite(){
            IPersistenceConfigurer db;
            db = new SQLiteConfiguration()
                .UseOuterJoin()
                .InMemory()
                .DoNot.ShowSql()
                .QuerySubstitutions("true 1, false 0, yes 'Y', no 'N'")
                .Raw("session_factory_name", TestEnvironment.TestSqlLiteFactoryName)
                .UseReflectionOptimizer()
                .Raw("command_timeout", "100")
                .Raw("current_session_context_class", typeof (TCurrentContext).AssemblyQualifiedName)
                .Raw("prepare_sql", "false")
                .Raw("hbm2ddl.keywords", "none")
                ;
            return db;
        }

        protected void DoAfterConfigure(Configuration cfg){
            if (AfterConfigure != null){
                AfterConfigure(this, new ConfigurationEventArgs(cfg));
            }
        }

        protected void DoBeforeConfigure(Configuration cfg, out bool configured){
            configured = false;
            if (BeforeConfigure == null){
                return;
            }
            var args = new ConfiguringEventArgs(cfg);
            BeforeConfigure(this, args);
            configured = args.Configured;
        }
        }
}