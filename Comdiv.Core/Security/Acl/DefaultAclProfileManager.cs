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
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Model;

namespace Comdiv.Security.Acl
{
    /// <summary>
    /// interface for local acl profile manager
    /// </summary>
    [DefaultImplementation(typeof(DefaultAclProfileManager))]
    public interface IAclProfileManager{
        /// <summary>
        /// loads all profiles, profiles with same codes in distinct levels are prioritized in usual order
        /// </summary>
        /// <returns></returns>
        IEnumerable<Entity> Enumerate();
        /// <summary>
        /// returns descriptor by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Entity Get(string code);
        /// <summary>
        /// creates or rewrites profile in USR zone from current acl profile with substitute of code and comment, 
        /// name creates automatically identical to [code].acl.config
        /// </summary>
        /// <returns></returns>
        IAclProfileManager SaveCurrentAs(string code, string comment);
        /// <summary>
        /// return current ACL profile descriptor
        /// </summary>
        /// <returns></returns>
        Entity GetCurrent();
        /// <summary>
        /// overwrites current ACL file with given profile
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        IAclProfileManager Activate(string code);
    }

    /// <summary>
    /// manages local ACL profiles in application
    /// </summary>
    public class DefaultAclProfileManager:IAclProfileManager
    {
        public DefaultAclProfileManager(){
            AclProfileFolder = "aclprofiles";
            CurrentAclFile = "~/usr/acl.config";
            SaveTargetFolder = "~/usr/aclprofiles/";
        }
        public string AclProfileFolder { get; set; }
        public string CurrentAclFile { get; set; }
        public string SaveTargetFolder { get; set; }

        public IEnumerable<Entity> Enumerate(){
            var result = new Dictionary<string, Entity>();
            foreach (var file in myapp.files.ResolveAll(AclProfileFolder,"*.*")){
                Entity ent= getEntityFromFile(file);
                if(!result.ContainsKey(ent.Code)){
                   
                    result[ent.Code] = ent;
                }
            }
            return result.Values;
        }

        public Entity Get(string code){
            if(code=="_current"){
                return GetCurrent();
            }
            return Enumerate().First(x => x.Code == code);
        }

        private static int i = 1;
        private Entity getEntityFromFile(string file) {
            var txt = myapp.files.Read(file);
            if(string.IsNullOrWhiteSpace(txt)) return null;
            try{
                var xml = XElement.Parse(txt);
                var code = xml.toStr("//name");
                var name = file;
                var comment = xml.toStr("//comment");
                return new Entity {Code = code, Name = name, Comment = comment};
            }
            catch(Exception ex){
                return new Entity {Name = file, Comment = ex.Message, Code = "ERROR" + i++};
            }

        }

        public IAclProfileManager SaveCurrentAs(string code,string comment){
            var txt = myapp.files.Read(CurrentAclFile);
            if(!string.IsNullOrWhiteSpace(code)){
                txt = txt.replace(@"<name>[^<]+?</name>", "<name>" + code + "</name>");
            }
            if(!string.IsNullOrWhiteSpace(comment)){
                txt = txt.replace(@"<comment>[^<]+?</comment>", "<comment>" + comment + "</comment>");
            }
            var fileName = SaveTargetFolder + code + ".acl.config";
            myapp.files.Write(fileName, txt);
            return this;
        }
        public Entity GetCurrent(){
            return getEntityFromFile(CurrentAclFile);
        }
        public IAclProfileManager Activate(string code){
            var profile = Enumerate().First(x => x.Code == code);
            var txt = myapp.files.Read(profile.Name);
            myapp.files.Write(CurrentAclFile, txt);
            return this;
        }
    }
}
