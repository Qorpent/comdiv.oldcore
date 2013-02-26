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
using System.Data;
using System.Data.Common;
using System.Threading;

namespace Comdiv.Model{
    public class StubCommand:DbCommand{
        private IDbCommand command;

        public StubCommand(IDbCommand command){
            this.command = command;
        }

        public override void Prepare(){
            command.Prepare();
        }

        public override string CommandText{
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public override int CommandTimeout{
            get { return command.CommandTimeout; }
            set { command.CommandTimeout = value; }
        }

        public override CommandType CommandType{
            get { return command.CommandType; }
            set { command.CommandType = value; }
        }

        public override UpdateRowSource UpdatedRowSource{
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value;}
        }

        protected override DbConnection DbConnection{
            get { return new StubConnection(command.Connection); }
            set { command.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection{
            get { throw new NotImplementedException(); }
        }

        protected override DbTransaction DbTransaction{
            get { return new StubTransaction(command.Transaction); }
            set { command.Transaction = value; }
        }

        public override bool DesignTimeVisible{
            get { return false;}
            set {  }
        }

        public override void Cancel(){
            command.Cancel();
        }

        protected override DbParameter CreateDbParameter(){
            throw  new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior){
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery(){
            return command.ExecuteNonQuery();
        }

        public override object ExecuteScalar(){
            return command.ExecuteScalar();
        }
    }
    public class StubTransaction:DbTransaction{
        private IDbTransaction transaction;

        public StubTransaction(IDbTransaction transaction){
            this.transaction = transaction;
        }
        public override void Commit(){
            transaction.Commit();
        }

        public override void Rollback(){
            transaction.Rollback();
        }

        protected override DbConnection DbConnection{
            get { return new StubConnection(transaction.Connection); }
        }

        public override IsolationLevel IsolationLevel{
            get { return transaction.IsolationLevel; }
        }
    }

    public class StubConnection:DbConnection{

        private IDbConnection connection;

        public StubConnection(IDbConnection realConnection){
            this.connection = realConnection;
        }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel){
            return new StubTransaction(connection.BeginTransaction(isolationLevel));
        }

        public override void Close(){
            connection.Close();
        }

        public override void ChangeDatabase(string databaseName){
            connection.ChangeDatabase(databaseName);
        }

        public override void Open(){
            connection.Open();
        }

        public override string ConnectionString{
            get { return connection.ConnectionString; }
            set { connection.ConnectionString = value; }
        }

        public override string Database{
            get { return connection.Database; }
        }

        public override ConnectionState State{
            get { return connection.State;}
        }

        public override string DataSource{
            get { return connection.Database; }
        }

        public override string ServerVersion{
            get { return "10.0"; }
        }

        protected override DbCommand CreateDbCommand(){
            return new StubCommand(connection.CreateCommand());
        }
    }
}