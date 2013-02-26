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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.Persistence
{
    public static class BaseDataExtensions
    {
		public static IList<IEntityDataPattern> QueryEntities<T>(this StorageWrapper<T> storage, string query)
		{
			if (query.Contains("select"))
			{
				var result = new List<IEntityDataPattern>();
				IList<object[]> tuples = storage.Query(query).Cast<object[]>().ToList();
				foreach (var tuple in tuples)
				{
					var item = new Entity {Code = tuple[0].toStr(), Name = tuple[1].toStr()};
					result.Add(item);
				}
				return result;

			}
			else
			{
				return storage.Query(query).OfType<IEntityDataPattern>().ToList();
			}
		}

        public static Table GetTable(this IDbConnection connection, string command,
                                     IDictionary<string, Object> parameters)
        {
            return Enumerable.FirstOrDefault(GetTables(connection, command, "", parameters));
        }

        public static IList<Table> GetTables(this IDbConnection connection, string command,
                                             IDictionary<string, Object> parameters){
            return GetTables(connection, command, "", parameters);
        }

        public static IList<Table> GetTables(this IDbConnection connection, string command, string password,
                                             IDictionary<string, Object> parameters)
        {
            connection.WellOpen();
            IDataReader reader = null;
            try
            {
                var com = connection.CreateCommand(command, parameters);
                com.CommandTimeout = 0;
                reader = com.ExecuteReader();
                return Table.GetTableSet(reader).ToArray();
            }
            finally
            {
                if (null != reader && !reader.IsClosed) reader.Close();
                connection.Close();
            }
        }

        public static void ExecuteNonQuery(this IDbConnection connection, string command,IParametersProvider provider) {
            var dict = null == provider ? null : new ParamMappingHelper().GetParameters(provider);
            ExecuteNonQuery(connection, command, dict);
        }
		public static T ExecuteScalar<T>(this IDbConnection connection, string command, IParametersProvider provider)
		{
			var dict = null == provider ? null : new ParamMappingHelper().GetParameters(provider);
			return ExecuteScalar<T>(connection, command, dict);
		}

        public static void ExecuteNonQuery(this IDbConnection connection, string command,
                                           IDictionary<string, object> parameters)
        {
           ExecuteNonQuery(connection,command,parameters,30);
        }

		public static void ExecuteNonQuery(this IDbConnection connection, string command,
										   IDictionary<string, object> parameters, int timeout)
		{
			connection.WellOpen();
			IDbCommand cmd = connection.CreateCommand(command, parameters);
			cmd.CommandTimeout = timeout;
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw new Exception("error in query:" + cmd.CommandText, ex);
			}
		}


        public static T ExecuteScalar<T>(this IDbConnection connection, string command,
                                         IDictionary<string, object> parameters)
        {
            return connection.ExecuteScalar(command, parameters, default(T));
        }

        public static T ExecuteScalar<T>(this IDbConnection connection, string command,
                                         IDictionary<string, object> parameters, T defValue)
        {
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var result = connection.CreateCommand(command, parameters).ExecuteScalar();
            if (null == result) return defValue;
            if (result is DBNull) return defValue;
            return result.to<T>();
        }

        public static object[] ExecuteRow(this IDbConnection connection, string command,
                                          IDictionary<string, object> parameters)
        {
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var reader = connection.CreateCommand(command, parameters).ExecuteReader(CommandBehavior.SingleRow);
            try
            {
                if (reader.Read())
                {
                    var result = new object[reader.FieldCount];
                    reader.GetValues(result);
                    return result;
                }
            }
            finally
            {
                reader.Close();
            }
            return null;
        }

        public static IDbConnection WellOpen(this IDbConnection connection){
            if (null == connection) return connection;
            if (ConnectionState.Open == connection.State) return connection;
            if (ConnectionState.Closed == connection.State){
                try {
                    connection.Open();
                    return connection;
                }catch(Exception ex) {
                    throw new Exception("cannot connect "+connection.ConnectionString);
                }
            }
            // TODO: если будет Exception то не ясно откуда он взялся 
            throw new InvalidOperationException("Insuficient connection state " + connection.State);
        }

        public static bool ObjectExisted(this IDbConnection connection, string objectName){
            var result = connection.ExecuteScalar<object>("select object_id('" + objectName + "')", (IParametersProvider)null);
            return !(null == result || result is DBNull);
        }

        public static bool HasRows(this IDbConnection connection, string objectName){
            var result = connection.ExecuteScalar<int>("select count(*) from " + objectName, (IParametersProvider)null);
            return 0 != result;
        }

        public static bool HasRows(this IDbConnection connection, string objectName, string whereCondition){
            if (whereCondition.noContent()) return connection.HasRows(objectName);
            var result = connection.ExecuteScalar<int>("select count(*) from {0} where {1}"._format(objectName, whereCondition), (IParametersProvider)null);
            return 0 != result;
        }

        public static IDbCommand CreateCommand(this IDbConnection connection, string command,
                                               object parameters_){
            var query = connection.CreateCommand();
	        query.CommandText = command;
			if(null!=parameters_) {
				if (parameters_ is IDictionary<string, object>) {
					var parameters = parameters_ as IDictionary<string, object>;
					if (parameters.Count != 0 && !command.Contains("@")) {
						command = command + " " +
						          parameters.Keys.Select(x => x.StartsWith("@") ? x : "@" + x).Select(x => x + "=" + x).concat(",");
					}
					query.CommandText = command;
					foreach (var pair in parameters) {
						var parameter = query.CreateParameter();
						parameter.ParameterName = pair.Key;
						if (pair.Value is DbType) parameter.DbType = (DbType) pair.Value;
						else {
							if (null == pair.Value) {
								parameter.Value = DBNull.Value;
							}
							else {
								parameter.Value = pair.Value;
								if (parameter.Value is XElement) {
									parameter.DbType = DbType.Xml;
									parameter.Value = ((XElement) parameter.Value).ToString();
								}
							}

						}


						query.Parameters.Add(parameter);
					}
				}
				else {
					var parameters = parameters_.GetType().GetProperties();
					if (parameters.Length != 0 && !command.Contains("@")) {
						command = command + " " +
						          parameters.Select(x => x.Name).Select(x => x.StartsWith("@") ? x : "@" + x).Select(x => x + "=" + x).
							          concat(",");
					}
					query.CommandText = command;
					foreach (var pair in parameters) {
						var parameter = query.CreateParameter();
						parameter.ParameterName = pair.Name;
						var val = pair.GetValue(parameters_);
						if (val is DbType) parameter.DbType = (DbType) val;
						else {
							if (null == val) {
								parameter.Value = DBNull.Value;
							}
							else {
								parameter.Value = val;
								if (parameter.Value is XElement) {
									parameter.DbType = DbType.Xml;
									parameter.Value = ((XElement) parameter.Value).ToString();
								}
							}

						}


						query.Parameters.Add(parameter);
					}
				}
			}
	        return query;
        }

        public static IDictionary<string, object> ExecuteDictionary(this IDbConnection connection, string command,
                                                                    IDictionary<string, object> parameters)
        {
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var reader = connection.CreateCommand(command, parameters).ExecuteReader(CommandBehavior.SingleRow);
            try
            {
                if (reader.Read())
                {
                    var result = new Dictionary<string, object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                        result[reader.GetName(i)] = reader[i];
                    return result;
                }
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
            return null;
        }

        public static T[] ExecuteOrm<T>(this IDbConnection connection, string command, IParametersProvider provider) where T:new() {
        	connection.WellOpen();
            var dict = null == provider ? null : new ParamMappingHelper().GetParameters(provider);
            return ExecuteOrm<T>(connection, command, dict);
        }

        public static T[] 
            ExecuteOrm<T>(this IDbConnection connection, string command,IDictionary<string, object> parameters) where T:new(){
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var result = new List<T>();
            var reader = connection.CreateCommand(command, parameters).ExecuteReader();
            try
            {
                while (reader.Read()||reader.NextResult()) {
                    var item = new T();
                    for(int i=0;i<reader.FieldCount;i++) {
                        var name = reader.GetName(i);
                        item.setPropertySafe(name, reader[i] is DBNull ? null : reader[i]);
                    }
                    result.Add(item);
                }
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
            return result.ToArray();
        }

        public static IDictionary<string, object> ExecuteDictionaryReader(this IDbConnection connection, string command,
                                                                    IDictionary<string, object> parameters)
        {
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var reader = connection.CreateCommand(command, parameters).ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                var result = new Dictionary<string, object>();
                while (reader.Read())
                {
                    if (reader.FieldCount == 2) {
                        result[reader[0] as string] = reader[1];
                    }else {
                        var subresult = new List<object>();
                        for(int i = 1; i< reader.FieldCount; i++) {
                            subresult.Add(reader[i]);
                        }
                        result[reader[0] as string] = subresult.ToArray();
                    }
                }
                return result;
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
            return null;
        }
		public static IDictionary<string, IList<T>> ExecuteDictionaryReaderList<T>(this IDbConnection connection, string command,
																   IDictionary<string, object> parameters) where T:struct 
		{
			if (null == connection) throw new ArgumentNullException("connection");
			connection.WellOpen();
			var reader = connection.CreateCommand(command, parameters).ExecuteReader(CommandBehavior.SingleResult);
			try
			{
				var result = new Dictionary<string,  IList<T>>();
				while (reader.Read()) {
					var key = reader[0] as string;
					var value = reader[1].to<T>();
					if(!result.ContainsKey(key)) {
						result[key] = new List<T>();
					}
					if(!result[key].Contains(value)) {
						result[key].Add(value);
					}
					
				}
				return result;
			}
			finally
			{
				reader.Close();
				connection.Close();
			}
			return null;
		}

        public static IList<T> ExecuteList<T>(this IDbConnection connection, string command,
                                                                   IDictionary<string, object> parameters) {
            var result = new List<T>();
            if (null == connection) throw new ArgumentNullException("connection");
            connection.WellOpen();
            var reader = connection.CreateCommand(command, parameters).ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                while (reader.Read()) {
                    result.Add(reader[0].to<T>());
                }
                return result;
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
        }
    }
}
