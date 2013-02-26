using System;
using System.Collections.Generic;
using System.Data;
using Comdiv.Collections;

namespace Comdiv.Persistence {
    public interface ISqlService {
        T ExecuteScalar<T>(string query, object parameters = null, string database = null, string system = "Default");
        Table ExecuteTable(string query, object parameters = null, string database = null, string system = "Default");
        void ExecuteNoQuery(string query, object parameters = null, string database = null, string system = "Default");

        T[] ExecuteOrm<T>(string query, object parameters = null, string database = null,
                          string system = "Default") where T : class,new();

        SqlBatchResult ExecuteBatch(string query, object parameters = null, string database = null, string system = "Default");
        string[] GetConnectionNames();
        string[] GetDatabaseNames(string database = null, string system = "Default");
        T[] ExecuteArray<T>(string query, object parameters = null, string database = null, string system = "Default" );
        IDictionary<K ,V> ExecuteDictionary<K,V>(string query, object parameters = null, string database = null, string system = "Default");

	    /// <summary>
	    /// Возвращает соединение
	    /// </summary>
	    /// <param name="system"></param>
	    /// <returns></returns>
	    /// <exception cref="InvalidOperationException"></exception>
	    IDbConnection GetConnection( string system);
    }
}