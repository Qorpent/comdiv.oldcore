using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Comdiv.Distribution;

using Comdiv.IO;

namespace Comdiv.MVC.Patching{
    public class SqlTask : IPackageInstallTask{
        public SqlTask(string connection, string script){
            Name = "sql";
            Connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connection].ConnectionString);
            if(Connection is SqlConnection){
                ((SqlConnection) Connection).InfoMessage += (s, a) => Builder.AppendLine(a.Message);
            }
            Commands = Regex.Split(script, @"(?i)[\r\n]+\s*(GO[^\S\r\n]*((\s+)|$))+", RegexOptions.Compiled|RegexOptions.ExplicitCapture).Where(s=>s.hasContent()).ToArray();
        }

        private StringBuilder builder = new StringBuilder();

        public string[] Commands { get; set; }

        public IDbConnection Connection { get; set; }

        #region IPackageInstallTask Members

        public string Name { get; set; }

        public StringBuilder Builder{
            get { return builder; }
            set { builder = value; }
        }

        public IPackageInstallResult Do(IPackage package, IFileSystem target){
            var result = new DefaultPackageInstallResult();
            result.State = ResultState.OK;
            Connection.Open();
            Builder = new StringBuilder();
            var trans = Connection.BeginTransaction();
            try{
                bool commit = true;
                foreach (var command in Commands){
                    var com = Connection.CreateCommand();
                    com.CommandType = CommandType.Text;
                    com.CommandText = command;
                    com.Transaction = trans;
                    com.ExecuteNonQuery();
                    if(command.Contains("//nocommit")){
                        commit = false;
                    }
                }
                if(commit)trans.Commit();
            }
            catch (Exception ex){
                trans.Rollback();
                logger.Kernel.Error("error in sql package task", ex);
                result.State = ResultState.Error;
            }
            finally{
                try{
                    Connection.Close();
                }catch
                {
                    
                }
                logger.Kernel.Info("sql:" + Builder.ToString());
            }
            //result.State = ResultState.Warning;
            return result;
        }

        #endregion
    }
}