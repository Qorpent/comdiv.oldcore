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
    /// ��������� ���������� �����������, ������������ ����������� �������
    /// comdiv.LookupRegistry � �������� ������� ����������
    /// <remarks>
    /// ��� �������������� DbLookup ������� � �������� �� ������������� ��
    /// NHib, AR ��� ADO, ���������� ������ ����������� � ������ � ��,
    /// ��������� ���������� ����� DbLookupRegistry (AR) � ������� �� �������
    /// ���� �� �����
    /// </remarks>
    /// </summary>
    public class DbLookupSourceProvider : ILookupSourceProvider, IReloadAble{
        //FOR IOC
        /// <summary>
        /// ��� ��������� � ��, ����������� ��������������� �����������
        /// </summary>
        public static string LookupProcedureName = "comdiv.Lookup";

        /// <summary>
        /// ��� ������� ������������ ������� �����������
        /// </summary>
        public static string LookupRegistryTableName = "comdiv.LookupRegistry";

        private SqlConnection connection;

        private string connectionString;
        private IEnumerable<ILookupSource> sources;

        /// <summary>
        /// ��� ������ �����������
        /// <remarks>������������ ���� ������ ����������� �������� �� ������</remarks>
        /// </summary>
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// �������� ������ ��� ��������� ����������
        /// <remarks>������������ ����� �������������� AR ���� �� ����������� ������ ����������� � �� ���������� ConnectionStringName</remarks>
        /// </summary>
        public string TargetTypeName { get; set; }

        //FOR CONNECTION DEFINITION

        /// <summary>
        /// ������ ����������� - ��������������� �������� ��� ����������� �� ConnectionStringName ��� TargetTypeName ����� AR, � ������ ������������� ���������� ����������� ����� �������, ���������� Default - ������ app.config//conncetions
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
        /// ��������� �������� � ������������ ���������� �� �� � �������������� ������� - ������������
        /// </summary>
        public IEnumerable<ILookupSource> Sources{
            get{
                lock (this){
                    //�� ������������ �� ������ ���, ������ � ������ ������ ��� ����� ������ Reload
                    if (null == sources){
                        sources = new List<ILookupSource>();

                        //����� ������� �������� � ���������� (���-�� sources IEnumerable)
                        var slist = (List<ILookupSource>) sources;

                        var connection = GetConnection();
                        var command = connection.CreateCommand();
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "select * from {0}"._format((object) LookupRegistryTableName);
                        connection.WellOpen();
                        using (var reader = command.ExecuteReader()){
                            while (reader.Read()){
                                //� ����������� �������� ���� ��� ����������� ������� � ����������
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
        /// ������������ ������� ���������� �� ��
        /// </summary>
        public void Reload(){
            //HACK: ����� -  ��� ��������� ������� Sources �������� ���������
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