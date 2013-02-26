using System.Collections.Generic;
using System.Data;
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
    /// Предназначен для унифицированного запроса данных из внешних таблиц
    /// предполагает наличие в целевой базе специальной хранимой процедуры
    /// comdiv.Lookup @alias nvarchar(255), @code nvarchar(255) = null, @name nvarchar(255) = null , @ioc.getmask bit = 0, @ioc.getmask bit = 1, @ioc.getparam nvarchar(max)
    /// хранимая процедура с данным именем должна возвращать один select с обязательными полями Id nvarchar(255), Name nvarchar(255), остальные поля не регламентированы 
    /// итоговый набор преобразуется в массив элементов LookupItem
    /// в параметрах процедуры:
    /// @alias - псевдоним неких данных
    /// @code - прямой код (по умолчанию) или маска
    /// @name - имя или маска имени (по умолчанию)
    /// @ioc.getmask - признак маски идентификтора
    /// @ioc.getmask - признак маски по имени
    /// @ioc.getparam - дополнительный параметр для делегации
    /// Процедура диспетчирует источники по псевдониму и формирует запросы по Id и/или Name с учетом масок
    /// <remarks>Класс сделан с рассчетом на использование исключительно вместе с DBLookupSourceProvider и не предназначен для прямого использования, поэтому этот класс internal</remarks>
    /// </summary>
    internal class DbLookupSource : BaseLookupSource{
        private readonly DbLookupSourceProvider parentProvider;

        internal DbLookupSource(DbLookupSourceProvider parentProvider){
            this.parentProvider = parentProvider;
        }

        /// <summary>
        /// Ссылка на родительский провайдер - используется для получения строки
        /// </summary>
        protected internal DbLookupSourceProvider ParentProvider{
            get { return parentProvider; }
        }

        /// <summary>
        /// Метод Select реализуется через вызов специальной хранимой процедуры
        /// </summary>
        /// <param name="query">Запрос подстановки</param>
        /// <returns>Результат подстановки из возвращенного select</returns>
        public override IEnumerable<ILookupItem> Select(ILookupQuery query){
            var connection = ParentProvider.GetConnection();

            //проверяем наличие процедуры в целевой БД
            if (!connection.ObjectExisted(DbLookupSourceProvider.LookupProcedureName)){
                //TODO: Test
                throw new ModelException(
                    "Can't find {0} procedure in db with {1} connection"._format(
                        DbLookupSourceProvider.LookupProcedureName,
                        connection.ConnectionString));
            }

            //определяем поведение DataReader
            var behaviour = CommandBehavior.Default;
            //если только первое значение, то выставить SingleRow
            if (query.First){
                behaviour = CommandBehavior.SingleRow;
            }

            //формируем команду
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = DbLookupSourceProvider.LookupProcedureName;
            //формируем параметры
            command.AddParameter("@alias", query.Alias);
            command.AddParameter("@code", query.Code);
            command.AddParameter("@name", query.Name);
            command.AddParameter("@code_mask", query.CodeMask);
            command.AddParameter("@name_mask", query.NameMask);
            command.AddParameter("@custom_param", query.Custom);

            //инициируем результат запроса и открываем соединение
            var result = new List<ILookupItem>();
            connection.WellOpen();

            using (var reader = command.ExecuteReader(behaviour)){
                while (reader.Read()){
                    //инициируем элемент подстановки и устанавливаем обязательные по спецификации поля
                    var item = new LookupItem
                               {Alias = query.Alias, Code = reader["Code"].ToString(), Name = reader["Name"].ToString()};

                    //инициируем дополнительные поля - не Code, не Name и включаем их в Properties
                    for (var i = 0; i < reader.FieldCount; i++){
                        var fieldName = reader.GetName(i);
                        if (fieldName.ToUpper().isIn("CODE", "NAME")){
                            continue;
                        }
                        item.Properties[fieldName] = reader[i];
                    }
                    result.Add(item);
                }
            }
            return result;
        }
    }
}