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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Logging;

namespace Comdiv.Persistence{
    [NoCover("trivial")]
    public class DefaultConnectionsSource : IConnectionsSource{
        #region IConnectionsSource Members
        public DefaultConnectionsSource() {
            myapp.OnReload += (s,a) => cacheddict = null;
        }

        public bool InWebContext { get; set; }
        private IDictionary<string, string> cacheddict;
        public IEnumerable<NamedConnection> GetConnections(){
            lock(this) {
                checkCache();
            }
            
            return cacheddict.Select(res => new NamedConnection(res.Key,res.Value));
        }

        public void Set(string id, string connection) {
            this.cacheddict[id] = connection;
        }

        public bool Exists(string id) {
            return Get(id).ConnectionString != null;
        }

        public NamedConnection Get(string id = "Default") {
            lock (this) {
                checkCache();
            }
            if(id.noContent()) {
                id = "Default";
            }
            return new NamedConnection(id, cacheddict.get(id,null));
        }

        private void checkCache() {
            if(null==cacheddict) {
                IDictionary<string, string> result = new Dictionary<string, string>();
                var customfiles = myapp.files.ResolveAll("", "connections.bxl", true);
                foreach (var customfile in customfiles)
                {
                    var strings = File.ReadAllLines(customfile);
                    foreach (var s in strings)
                    {
                        var match = Regex.Match(s, @"^(\w+)\s+[""']([^""']+)[""']\s*$");
                        if (match.Success)
                        {
                            var name = match.Groups[1].Value;
                            var connection = match.Groups[2].Value;
                            if (!result.ContainsKey(name))
                            {
                                result[name] = connection;
                            }
                        }
                    }
                }

                if (InWebContext) {
                    var file = myapp.files.Read("~/usr/connections.config");
                    if(file.hasContent()) {
                        var x = XElement.Parse(file);
                        var connections = x.Elements("add");
                        foreach (var connection in connections) {
                            var name = connection.attr("name");
                            var cs = connection.attr("connectionString");
                            if (!result.ContainsKey(name))
                            {
                                result[name] = cs;
                            }
                        }
                    }
                }
                else {

                    foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings) {
                        logger.get("comdiv.persistence.connectionssource").debug(
                            () => "finded connection " + connectionString.Name);
                        if (connectionString.Name.ToLower() == "localsqlserver"
                            || connectionString.Name.ToLower() == "localsqliteserver") {
                            continue;
                        }
                        if (!result.ContainsKey(connectionString.Name)) {
                            result[connectionString.Name] = connectionString.ConnectionString;
                        }

                    }
                }
                cacheddict = result;
            }
        }

        #endregion
    }
}