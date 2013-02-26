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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Comdiv.Extensions;

namespace Comdiv.Persistence{
    public class NamedConnection:IConnectionsSource{
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public NamedConnection(string name,string connection){
            this.Name = name;
            this.ConnectionString = connection;
        }

        public NamedConnection(){
            
        }

        public IEnumerable<NamedConnection> GetConnections(){
            if (string.IsNullOrWhiteSpace(Name) && (ConnectionString ?? "").Length < 20){
                string real = "";
                try{
                    real = ConfigurationManager.ConnectionStrings[this.Name].ConnectionString;
                }
                catch (Exception){
                    
                }
                if (string.IsNullOrWhiteSpace(real)){
                    this.ConnectionString = real;
                }
                
            }
            yield return this;
        }

        public NamedConnection Get(string id) {
            if(this.Name==id) {
                return this;
            }
            return null;
        }

        public void Set(string id, string connection) {
            this.Name = id;
            this.ConnectionString = connection;
        }

        public bool Exists(string id) {
            return this.Name == id && this.ConnectionString != null;
        }

        public IDbConnection CreateConnection() {
            //NOTE: Sql Server suppoort only for now
            return new SqlConnection(this.ConnectionString);
        }
    }
}