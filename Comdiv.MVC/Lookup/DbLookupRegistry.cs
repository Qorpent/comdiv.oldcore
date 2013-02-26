using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Mapping;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using FluentNHibernate;

namespace Comdiv.Model.Lookup{
    public interface IDbLookupRegistry : IEntityDataPattern{
        /// <summary>
        /// Псевдоним источника
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Реализующий источник объект
        /// </summary>
        string DataObject { get; set; }

        /// <summary>
        /// Маркер того, что объект - хранимая процедура
        /// </summary>
        bool IsProc { get; set; }
    }

    /// <summary>
    /// AR класс для работы с таблицей comdiv.LookupRegistry, адаптирует
    /// стандартные имена полей Code,Name с сохранением соответствия
    /// IEntityDataPattern и при этом дает вызывающему коду
    /// корректные имена Alias и DataObject,
    /// предполагается что класс используется для администрирования
    /// </summary>
    public class DbLookupRegistry : IDbLookupRegistry{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }
        #region IDbLookupRegistry Members

        /// <summary>
        /// Псевдоним источника
        /// </summary>
        public virtual string Alias{
            get { return Code; }
            set { Code = value; }
        }

        /// <summary>
        /// Реализующий источник объект
        /// </summary>
        public virtual string DataObject{
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>
        /// Маркер того, что объект - хранимая процедура
        /// </summary>
        public virtual bool IsProc { get; set; }

        public virtual int Id { get; set; }

        public virtual string Comment { get; set; }

        public virtual DateTime Version { get; set; }

        /// <summary>
        /// Спрятанная реализация Name, дублер DataObject
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Спрятанная реализация Code, дублер Alias
        /// </summary>
        public virtual string Code { get; set; }

        #endregion
    }

    public class DBLookupRegistryMap : ExtendedClassMap<DbLookupRegistry>{
        public DBLookupRegistryMap() : this("comdiv") {}

        public DBLookupRegistryMap(string schema) : base(schema){
            #if LIB2
            Table("LookupRegistry");
            #else
            WithTable("LookupRegistry");
            #endif
            this.standard();
        }
    }

    public class DBLookupModel : PersistenceModel{
        public DBLookupModel(){
            Add(new DBLookupRegistryMap());
        }
    }
}