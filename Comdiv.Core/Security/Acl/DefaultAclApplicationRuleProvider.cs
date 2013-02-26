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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comdiv.IO;

namespace Comdiv.Security.Acl{
    public class DefaultAclApplicationRuleProvider : IAclApplicationRuleProvider
    {
        private static string fileName = "acl.config";
        private static string usrFileName = "~/usr/" + fileName;

        public DefaultAclApplicationRuleProvider(){
            Idx = 100;
        }
        public IFilePathResolver FileSystem { get; set; }
        public IEnumerable<IAclRule> GetRules(){
            return getRulesInternal().ToArray();
        }

        private IEnumerable<IAclRule> getRulesInternal(){
            if (null == FileSystem) yield break;
            var serializer = new AclRuleXmlSerializer();
            foreach(string file in FileSystem.ResolveAll(fileName)){
                var text = FileSystem.Read(file);
                foreach (var s in serializer.Read(text)){
                    s.Evidence = "local("+Path.GetFileName(Path.GetDirectoryName(file))+")";
                    yield return s;
                }
            }
        }

        public int Idx { get; set; }

        public void Add(IAclRule rule){
            
            var olddata = FileSystem.Read(usrFileName);
            var serializer = new AclRuleXmlSerializer();
            FileSystem.Write(usrFileName, serializer.Add(olddata, rule));
        }

        public void Remove(IAclRule rule){
            var olddata = FileSystem.Read(usrFileName);
            var serializer = new AclRuleXmlSerializer();
            FileSystem.Write(usrFileName, serializer.Remove(olddata, rule));
        }

        public void Clear(){
            FileSystem.Write(usrFileName, "<rules />");
        }
    }
}