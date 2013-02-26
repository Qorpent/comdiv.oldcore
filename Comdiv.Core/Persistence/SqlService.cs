using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Extensions;
using Comdiv.Inversion;
using mia.utils;

namespace Comdiv.Persistence
{
    public class SqlService : ISqlService {
        public T ExecuteScalar<T>(string query,object parameters = null, string database = null, string system = "Default") {
            var result = default(T);
            execute(query,database,system,parameters,c=>result=c.ExecuteScalar().to<T>());
            return result;
        }
        public Table ExecuteTable(string query,object parameters = null, string database = null, string system = "Default")
        {
            Table result = null;
            execute(query,database, system, parameters, c =>{
                var reader = c.ExecuteReader();
                var result_ =  Table.GetTableSet(reader).ToArray();
				if(0==result_.Length) {
					result = new Table();
				}else {
					result = result_[0];
				}
                reader.Close();
                                                 });
            return result;
        }

        public T[] ExecuteArray<T>(string query, object parameters = null, string database = null, string system = "Default" ) {
            IList<T> result = new List<T>();
            execute(query, database, system, parameters, c =>
            {
                using(var reader = c.ExecuteReader()) {
                    while (reader.Read()) {
                        result.Add(reader[0].to<T>());
                    }   
                }
            });
            return result.ToArray();
        }

        public IDictionary<K ,V> ExecuteDictionary<K,V>(string query, object parameters = null, string database = null, string system = "Default") {
            var result = new Dictionary<K, V>();
            execute(query, database, system, parameters, c =>
            {
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[reader[0].to<K>()]=(reader[1].to<V>());
                    }
                }
            });
            return result;
        }

        public void ExecuteNoQuery(string query, object parameters = null, string database = null, string system = "Default")
        {
            execute(query,database, system, parameters, c => c.ExecuteNonQuery());
        }

        public T[] ExecuteOrm<T>(string query,object parameters, string database, string system) where T:class,new(){
            var result = new List<T>();
            IDictionary<string , PropertyInfo> props = new Dictionary<string, PropertyInfo>();
            IDictionary<string , FieldInfo> fields = new Dictionary<string, FieldInfo>();
            var type = typeof (T);
            execute(query,database,system,parameters,
                c=>
                    {
                        var r = c.ExecuteReader();
                        for(int i = 0;i<r.FieldCount;i++) {
                            var name = r.GetName(i);
                            var prop = type.GetProperty(name,
                                                        BindingFlags.SetProperty | BindingFlags.NonPublic |
                                                        BindingFlags.Public | BindingFlags.Instance |
                                                        BindingFlags.IgnoreCase);
                            if(prop!=null) {
                                props[name] = prop;
                                continue;
                                
                            }
                            var field = type.GetField(name,
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Public | BindingFlags.Instance |
                                                        BindingFlags.IgnoreCase);
                            if(null!=field) {
                                fields[name] = field;
                            }
                        }
                        while (r.Read()) {
                            var item = new T();
                            foreach (var p in props) {
                                p.Value.SetValue(item,r[p.Key].to(p.Value.PropertyType),null);
                            }
                            foreach (var f in fields) {
                                f.Value.SetValue(item,r[f.Key].to(f.Value.FieldType));
                            }
                            result.Add(item);
                        }
                    }
                );
            return result.ToArray();
        }

        public SqlBatchResult ExecuteBatch(string query, object parameters = null, string database = null, string system = "Default")
        {
            var result = new SqlBatchResult();
            execute(query, database,system, parameters, c =>{
                                                     var scripts = new SqlScriptParser().Parse(query);
                                                     foreach (var command in scripts) {
                                                         c.CommandText = command;
                                                         var r = c.ExecuteReader();
                                                         result.ResultSet.addRange(Table.GetTableSet(r).ToArray());
                                                         r.Close();
                                                     }
                                                 },result.Log);
            return result;
        }
        IList<string > log = new List<string>();

        public string[] GetConnectionNames() {
            return
                (myapp.ioc.get<IConnectionsSource>() ?? new DefaultConnectionsSource()).GetConnections().Select(
                    x => x.Name).ToArray();
        }

        public string[] GetDatabaseNames(string database = null, string system = "Default") {
            var result = ExecuteTable("select name from sys.databases where database_id > 4 order by name", null, database, system);
            return result.Rows.Select(x => x.Values[0]).Cast<string>().ToArray();
        }

        protected void execute(string query, string database, string system, object parameters, Action<IDbCommand> action, IList<string> mylog = null)
        {
            parameters = parameters ?? new Dictionary<string, object>();
            mylog = mylog ?? log;
            IDbConnection connection = GetConnection(system);
            log.Clear();
            if(connection is SqlConnection) {
                ((SqlConnection)connection).InfoMessage += (sender, e) => mylog.Add(e.Message);
            }
            using (connection) {
                connection.Open();
                if (database.hasContent())
                {
                    connection.ChangeDatabase(database);
                }
                var command = connection.CreateCommand(query, parameters);
                command.Connection = connection;
                action(command);
            }
        }

	    /// <summary>
	    /// Возвращает соединение
	    /// </summary>
	    /// <param name="system"></param>
	    /// <returns></returns>
	    /// <exception cref="InvalidOperationException"></exception>
	    public IDbConnection GetConnection( string system)
        {
            if(system.Contains("Initial Catalog"))
            {
                return new SqlConnection(system);
            }
            system = system.hasContent() ? system : "Default";
            var connectionprovider = myapp.ioc.get<IConnectionsSource>() ?? new DefaultConnectionsSource();
            var connectiondescription = connectionprovider.GetConnections().Where(x => x.Name.ToLower() == system.ToLower()).FirstOrDefault();
            if(null==connectiondescription) {
                throw new InvalidOperationException("Невозможно найти соединение с именем " + system);
            }
            var result = connectiondescription.CreateConnection();
            
            return result;
        }

        
    }
}
