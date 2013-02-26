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
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Model.Interfaces;

namespace Comdiv.Security.Acl{
    public abstract class XmlBasedTokenProvider<T> : IAclTokenResolverImpl where T:class,IWithCode{
        public readonly IDictionary<string, string> Prefixes = new Dictionary<string, string>();

        public virtual string GetToken(string code){
            return GetToken(typeof(T), code);
        }

        public bool Initialized { get; set; }
        public bool Loaded { get; set;}
        public string DefaultPrefix { get; set; }
        public string FileName { get; set; }
        

        public int Idx
        {
            get; set;
        }

        public void Initialize(){
            myapp.OnReload += applicationCenter_OnReload;
            Initialized = true;
        }

        void applicationCenter_OnReload(object sender, Comdiv.Common.EventWithDataArgs<int> args)
        {
            Loaded = false;
        }

        public void ReloadPrefixes(){
            Prefixes.Clear();
            var xml = myapp.files.ReadXml(FileName, String.Empty,
                                          new ReadXmlOptions {Merge = true});
            if (!string.IsNullOrWhiteSpace(xml)){
                var reports = XElement.Parse(xml).XPathSelectElements("//r");
                foreach (var report in reports){

                    if (!Prefixes.ContainsKey(report.attr("c"))){
                        var prefix = String.Empty;
                        XElement p = report;
                        while (null != (p = p.Parent)){
                            if (p.Name == "p"){
                                prefix = p.attr("n") + "/" + prefix;
                            }
                            //p = p.Parent;
                        }
                        Prefixes[report.attr("c")] = prefix.Remove(prefix.Length - 1);
                    }
                }
            }
            Loaded = true;
        }

        public void CheckState(){
            if(!Initialized)Initialize();
            if(!Loaded)ReloadPrefixes();
        }

        public string GetToken(object aclTarget){
            if(typeof(T).IsAssignableFrom( aclTarget.GetType())){
                CheckState();
                var code = ((T)aclTarget).Code;
                return string.Format("/{0}/{1}/{2}", prefix(code), code, suffix((T)aclTarget));
            }
            return null;
        }

        public virtual string suffix(T o){
            return String.Empty;
        }

        public string prefix(string code){
            var result = DefaultPrefix;
            if(Prefixes.ContainsKey(code)){
                result += "/" + Prefixes[code];
            }
            return result;
        }

        public string GetToken(Type aclType, string aclId){
            if (typeof(T).IsAssignableFrom(aclType))
            {
                CheckState();
                return string.Format("/{0}/{1}/", prefix(aclId), aclId);
            }
            return null;
        }
    }
}