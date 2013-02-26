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
        /// ��������� ���������
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// ����������� �������� ������
        /// </summary>
        string DataObject { get; set; }

        /// <summary>
        /// ������ ����, ��� ������ - �������� ���������
        /// </summary>
        bool IsProc { get; set; }
    }

    /// <summary>
    /// AR ����� ��� ������ � �������� comdiv.LookupRegistry, ����������
    /// ����������� ����� ����� Code,Name � ����������� ������������
    /// IEntityDataPattern � ��� ���� ���� ����������� ����
    /// ���������� ����� Alias � DataObject,
    /// �������������� ��� ����� ������������ ��� �����������������
    /// </summary>
    public class DbLookupRegistry : IDbLookupRegistry{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }
        #region IDbLookupRegistry Members

        /// <summary>
        /// ��������� ���������
        /// </summary>
        public virtual string Alias{
            get { return Code; }
            set { Code = value; }
        }

        /// <summary>
        /// ����������� �������� ������
        /// </summary>
        public virtual string DataObject{
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>
        /// ������ ����, ��� ������ - �������� ���������
        /// </summary>
        public virtual bool IsProc { get; set; }

        public virtual int Id { get; set; }

        public virtual string Comment { get; set; }

        public virtual DateTime Version { get; set; }

        /// <summary>
        /// ���������� ���������� Name, ������ DataObject
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// ���������� ���������� Code, ������ Alias
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