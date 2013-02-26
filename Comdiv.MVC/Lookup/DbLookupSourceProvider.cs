using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Model.Lookup{
    /// <summary>
    /// Поставщик источников подстановки, использующий стандартную таблицу
    /// comdiv.LookupRegistry в качестве реестра источников
    /// <remarks>
    /// Вся инфраструктура DbLookup создана с прицелом на независимость от
    /// NHib, AR или ADO, использует прямое подключение и работу с БД,
    /// дублирует персистент класс DbLookupRegistry (AR) и обоюдно не зависят
    /// друг от друга
    /// </remarks>
    /// </summary>
    public class DbLookupSourceProvider : ILookupSourceProvider, IReloadAble{
        //FOR IOC
        /// <summary>
        /// Имя процедуры в БД, реализующей диспетчирование подстановок
        /// </summary>
        public static string LookupProcedureName = "comdiv.Lookup";

        /// <summary>
        /// Имя таблицы стандартного реестра подстановок
        /// </summary>
        public static string LookupRegistryTableName = "comdiv.LookupRegistry";

        private SqlConnection connection;

        private string connectionString;
        private IEnumerable<ILookupSource> sources;

        /// <summary>
        /// Имя строки подключения
        /// <remarks>Используется если строка подключения напрямую не задана</remarks>
        /// </summary>
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Корневой объект для настройки исключения
        /// <remarks>Используется через инфраструктуру AR если не установлена строка подключения и не установлен ConnectionStringName</remarks>
        /// </summary>
        public string TargetTypeName { get; set; }

        //FOR CONNECTION DEFINITION

        /// <summary>
        /// Строка подключения - устанавливается напрямую или вычисляется из ConnectionStringName или TargetTypeName через AR, в случае невозможности установить подключение таким образом, использует Default - запись app.config//conncetions
        /// </summary>
        public string ConnectionString{
            get{
                if (connectionString.noContent()){
                    if (ConnectionStringName.hasContent()){
                        //TODO: Test
                        connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
                    }
                    else if (TargetTypeName.hasContent()){
                        //TODO: Test
                        connectionString = Container.getConnectionString();
                    }
                    else{
                        //HACK: for Default connection
                        connectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
                    }
                }
                return connectionString;
            }
            set { connectionString = value; }
        }

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

        #region ILookupSourceProvider Members

        public int Priority { get; set; }

        /// <summary>
        /// Выполняет загрузку и формирование источников ИЗ БД с использованием таблицы - регистратора
        /// </summary>
        public IEnumerable<ILookupSource> Sources{
            get{
                lock (this){
                    //не перечитываем БД каждый раз, только в начале работы или после метода Reload
                    if (null == sources){
                        sources = new List<ILookupSource>();

                        //чтобы удобнее работать с переменной (так-то sources IEnumerable)
                        var slist = (List<ILookupSource>) sources;

                        var connection = GetConnection();
                        var command = connection.CreateCommand();
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "select * from {0}"._format((object) LookupRegistryTableName);
                        connection.WellOpen();
                        using (var reader = command.ExecuteReader()){
                            while (reader.Read()){
                                //в конструктор передаем себя для обеспечения доступа к соединению
                                var source = new DbLookupSource(this);
                                source.Alias = reader["Code"] as string;
                                source.Comment = reader["Comment"] as string;
                                slist.Add(source);
                            }
                        }
                    }
                    return sources;
                }
            }
        }

        #endregion

        #region IReloadAble Members

        /// <summary>
        /// Перечитывает таблицу источников из БД
        /// </summary>
        public void Reload(){
            //HACK: лэзик -  при следующем запросу Sources обновить коллекцию
            sources = null;
        }

        #endregion

        internal IDbConnection GetConnection(){
            //  @"ConnectionString".ioc.getHasContent(ConnectionString);
            //WDES: Supports mssql connections only
            return connection ?? (connection = new SqlConnection(ConnectionString + ";Application Name=lookuper"));
        }
    }
}