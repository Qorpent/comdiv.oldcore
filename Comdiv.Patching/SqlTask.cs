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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Logging;

namespace Comdiv.Patching{
    public class SqlTask : IPackageInstallTask{
        private StringBuilder builder = new StringBuilder();

        public SqlTask(string connection, string script){
            Name = "sql";
            Connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connection].ConnectionString);
            if (Connection is SqlConnection){
                ((SqlConnection) Connection).InfoMessage += (s, a) => Builder.AppendLine(a.Message);
            }
            Commands =
                Regex.Split(script, @"(?i)[\r\n]+\s*(GO[^\S\r\n]*((\s+)|$))+",
                            RegexOptions.Compiled | RegexOptions.ExplicitCapture).Where(s => s.hasContent()).ToArray();
        }

        public string[] Commands { get; set; }

        public IDbConnection Connection { get; set; }

        public StringBuilder Builder{
            get { return builder; }
            set { builder = value; }
        }

        #region IPackageInstallTask Members

        public string Name { get; set; }

        public IPackageInstallResult Do(IPackage package, IFilePathResolver target){
            var result = new DefaultPackageInstallResult();
            result.State = ResultState.OK;
            Connection.Open();
            Builder = new StringBuilder();
            var trans = Connection.BeginTransaction();
            try{
                var commit = true;
                foreach (var command in Commands){
                    var com = Connection.CreateCommand();
                    com.CommandType = CommandType.Text;
                    com.CommandText = command;
                    com.Transaction = trans;
                    com.ExecuteNonQuery();
                    if (command.Contains("//nocommit")){
                        commit = false;
                    }
                }
                if (commit){
                    trans.Commit();
                }
            }
            catch (Exception ex){
                trans.Rollback();
                logger.get("comdiv.core").Error("error in sql package task", ex);
                result.State = ResultState.Error;
            }
            finally{
                try{
                    Connection.Close();
                }
                catch{}
                logger.get("comdiv.core").Info("sql:" + Builder);
            }
            //result.State = ResultState.Warning;
            return result;
        }

        #endregion
    }
}