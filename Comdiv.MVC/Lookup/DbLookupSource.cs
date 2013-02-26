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
    /// ������������ ��� ���������������� ������� ������ �� ������� ������
    /// ������������ ������� � ������� ���� ����������� �������� ���������
    /// comdiv.Lookup @alias nvarchar(255), @code nvarchar(255) = null, @name nvarchar(255) = null , @ioc.getmask bit = 0, @ioc.getmask bit = 1, @ioc.getparam nvarchar(max)
    /// �������� ��������� � ������ ������ ������ ���������� ���� select � ������������� ������ Id nvarchar(255), Name nvarchar(255), ��������� ���� �� ���������������� 
    /// �������� ����� ������������� � ������ ��������� LookupItem
    /// � ���������� ���������:
    /// @alias - ��������� ����� ������
    /// @code - ������ ��� (�� ���������) ��� �����
    /// @name - ��� ��� ����� ����� (�� ���������)
    /// @ioc.getmask - ������� ����� �������������
    /// @ioc.getmask - ������� ����� �� �����
    /// @ioc.getparam - �������������� �������� ��� ���������
    /// ��������� ������������ ��������� �� ���������� � ��������� ������� �� Id �/��� Name � ������ �����
    /// <remarks>����� ������ � ��������� �� ������������� ������������� ������ � DBLookupSourceProvider � �� ������������ ��� ������� �������������, ������� ���� ����� internal</remarks>
    /// </summary>
    internal class DbLookupSource : BaseLookupSource{
        private readonly DbLookupSourceProvider parentProvider;

        internal DbLookupSource(DbLookupSourceProvider parentProvider){
            this.parentProvider = parentProvider;
        }

        /// <summary>
        /// ������ �� ������������ ��������� - ������������ ��� ��������� ������
        /// </summary>
        protected internal DbLookupSourceProvider ParentProvider{
            get { return parentProvider; }
        }

        /// <summary>
        /// ����� Select ����������� ����� ����� ����������� �������� ���������
        /// </summary>
        /// <param name="query">������ �����������</param>
        /// <returns>��������� ����������� �� ������������� select</returns>
        public override IEnumerable<ILookupItem> Select(ILookupQuery query){
            var connection = ParentProvider.GetConnection();

            //��������� ������� ��������� � ������� ��
            if (!connection.ObjectExisted(DbLookupSourceProvider.LookupProcedureName)){
                //TODO: Test
                throw new ModelException(
                    "Can't find {0} procedure in db with {1} connection"._format(
                        DbLookupSourceProvider.LookupProcedureName,
                        connection.ConnectionString));
            }

            //���������� ��������� DataReader
            var behaviour = CommandBehavior.Default;
            //���� ������ ������ ��������, �� ��������� SingleRow
            if (query.First){
                behaviour = CommandBehavior.SingleRow;
            }

            //��������� �������
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = DbLookupSourceProvider.LookupProcedureName;
            //��������� ���������
            command.AddParameter("@alias", query.Alias);
            command.AddParameter("@code", query.Code);
            command.AddParameter("@name", query.Name);
            command.AddParameter("@code_mask", query.CodeMask);
            command.AddParameter("@name_mask", query.NameMask);
            command.AddParameter("@custom_param", query.Custom);

            //���������� ��������� ������� � ��������� ����������
            var result = new List<ILookupItem>();
            connection.WellOpen();

            using (var reader = command.ExecuteReader(behaviour)){
                while (reader.Read()){
                    //���������� ������� ����������� � ������������� ������������ �� ������������ ����
                    var item = new LookupItem
                               {Alias = query.Alias, Code = reader["Code"].ToString(), Name = reader["Name"].ToString()};

                    //���������� �������������� ���� - �� Code, �� Name � �������� �� � Properties
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