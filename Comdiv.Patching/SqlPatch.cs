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
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Patching{
    public class SqlPatch : DefaultPatch{
        private string connection = "Default";
        private string script;

        public SqlPatch(){
            AutoLoad = true;
            ReInstallable = true;
        }

        public string Connection{
            get { return connection; }
            set { connection = value; }
        }

        public override string Code{
            get{
                if (base.Code.noContent()){
                    Code = Connection + ":" + FileName;
                }
                return base.Code;
            }
            set { base.Code = value; }
        }

        public string FileName { get; set; }

        public string Script{
            get{
                if (script.noContent()){
                    try{
                        script = "";
                        foreach (var file in FileName.split()){
                            script += Manager.PathResolver.Read(file);
                            script += "\r\nGO\r\n";
                        }
                    }
                    catch (Exception ex){
                        throw new Exception("Ошибка загрузки файла " + FileName, ex);
                    }
                }
                return script;
            }
            set { script = value; }
        }

        public override IList<IPackageInstallTask> Tasks{
            get{
                if (base.Tasks.Count == 0){
                    base.Tasks.Add(new SqlTask(Connection, Script));
                }
                return base.Tasks;
            }
        }
    }
}