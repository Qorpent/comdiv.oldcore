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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Comdiv.Extensions;

using System.Xml.XPath;
namespace Comdiv.Security.Acl{
    public class AclRuleXmlSerializer{
        public IEnumerable<IAclRule> Read(string text){
            var xml = XElement.Parse(text);
            var rules = xml.Elements();
            foreach (var rule in rules){



                if (new[]{"allow", "deny", "require"}.Contains(rule.Name.ToString())){
                    yield return new AclRule{
                                                Permissions = rule.attr("permission", String.Empty),
                                                PrincipalMask = rule.attr("principal", String.Empty),
                                                RuleType =
                                                    (AclRuleType)
                                                    Enum.Parse(typeof (AclRuleType), rule.Name.ToString(), true),
                                                System = rule.attr("system"),
                                                TokenMask = rule.attr("token"),
                                                Active = rule.attr("active", "true").toBool(),
                                                StartDate = rule.attr("start", "01.01.1900").toDate(),
                                                EndDate = rule.attr("end", "01.01.3000").toDate(),
                                            };

                }
            }
        }


        public string Add(string text,IAclRule rule){
            var list = Read(text).ToList();
            if(!list.Contains(rule)){
                list.Add(rule);
            }
            return Write(list);
            
        }
        public string Remove(string text, IAclRule rule)
        {
            var list = Read(text).ToList();
            list.Remove(rule);
            return Write(list);

        }

        public string Write(IEnumerable<IAclRule> rules){
            var sw = new StringWriter();
            var xw = XmlWriter.Create(sw);
            xw
                .open("rules")
                .map(rules,(x,r)=>{
                               x
                                   .open(r.RuleType.ToString().ToLower());
                               x .attr("token",r.TokenMask)
                                   .attr("system",r.System)
                                   .attr("principal", r.PrincipalMask)
                                   .attr("permission",r.Permissions)
                                   .attr("active",r.Active.ToString())
                                   .attr("start",r.StartDate.ToString("dd.MM.yyyy hh:mm"))
                                   .attr("end", r.EndDate.ToString("dd.MM.yyyy hh:mm"))
                                   .close()
                                   ;
                           })
                .close()
                ;
            xw.Flush();
            return sw.ToString();
        }
    }
}