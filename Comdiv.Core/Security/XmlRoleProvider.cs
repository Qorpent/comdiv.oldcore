// Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  Supported by Media Technology LTD 
//   
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Qorpent.Events;
using Qorpent.Mvc;
using Qorpent.Security;

namespace Comdiv.Security{
    /// <summary>
    ///   Can work with security.map.config file
    /// </summary>
    public class XmlRoleProvider<T> : IRoleResolverExtension,IResetable where T : class, IApplicationXmlReader, new(){
        private bool loaded;
        protected IApplicationXmlReader xmlreader;

        public XmlRoleProvider() : this(null){
        }

        public XmlRoleProvider(T xreader){
            xmlreader = xreader ?? myapp.ioc.get<IApplicationXmlReader>() ?? new T();
        }

        protected List<string> ApplicationRoles { get; set; }

        protected StringMap RoleMap { get; set; }

        protected StringMap UserMap { get; set; }

        
      

	    /// <param name="principal"> </param>
	    /// <param name="role"> </param>
	    /// <param name="exact"> </param>
	    /// <param name="callcontext"> </param>
	    /// <param name="customcontext"> </param>
	    /// <returns> </returns>
	    public bool IsInRole(IPrincipal principal, string role, bool exact = false, IMvcContext callcontext = null, object customcontext = null) {
			if (!loaded)
			{
				reload();
			}

			if (UserMap[principal.Identity.Name.ToUpper()].Contains(role.ToUpper()))
			{
				return true;
			}
			var un = UserName.For(principal);

			if (UserMap[un.Domain.ToUpper()].Contains(role.ToUpper()))
			{
				return true;
			}


			return RoleMap.ReverseAll(role).Any(outerRole => this.IsInRole(principal, outerRole) || myapp.roles.IsInRole(principal, outerRole));
	    }

	    public IEnumerable<string> GetRoles(){
            if (!loaded){
                reload();
            }
            return ApplicationRoles.ToArray();
        }

	    public bool IsExclusiveSuProvider { get { return false; }}


        protected virtual void reload(){
            var src = GetSource();
            ApplicationRoles = new List<string>();
            UserMap = new StringMap();
            RoleMap = new StringMap();
            LoadMappings(src);
            loaded = true;
        }

        protected virtual XElement GetSource(){
            var src = xmlreader.Read("security.map.config");
            prepareResult(src);
            return src;
        }

        private void loadApplicationRoles(XElement element){
            ApplicationRoles = new List<string>();
            var roles = element.XPathSelectElements("//role");
            foreach (var role in roles){
                var roleName = role.get("name", String.Empty);

                if (!ApplicationRoles.Contains(roleName)){
                    ApplicationRoles.Add(roleName);
                }
            }
        }

        private void LoadMappings(XElement x){
            loadApplicationRoles(x);
            loadMaps(x);
        }

        private void loadMaps(XElement element){
            UserMap.Clear();
            var userMaps = element.XPathSelectElements("//map[@user]");
            foreach (var map in userMaps){
                UserMap.Add(map.get("user", String.Empty).ToUpper(), map.get("as", String.Empty).ToUpper());
            }

            RoleMap.Clear();
            var roleMaps = element.XPathSelectElements("//map[@role]");
            foreach (var map in roleMaps){
                RoleMap.Add(map.get("role", String.Empty).ToUpper(), map.get("as", String.Empty).ToUpper());
            }
        }

        protected virtual void prepareResult(XElement result){
            //copy code attribute to name attribute in roles
            foreach (var role in result.Elements("role")){
                if (!string.IsNullOrWhiteSpace(role.attr("name"))){
                    continue;
                }
                var name = role.attr("code");
                role.Add(new XAttribute("name", name));
            }
            //parse map attribute
            //UPPERCASE are used as role mappings
            //lowercase or that contains '/' as user mappings
            foreach (var map in result.Elements("map")){
                if (!string.IsNullOrWhiteSpace(map.attr("as"))){
                    continue;
                }
                var from = map.attr("id");
                var to = map.attr("name");
                map.Add(new XAttribute("as", to));
                if (from == from.ToLower() || from.Contains("/")){
                    map.Add(new XAttribute("user", from.Replace("/", "\\")));
                }
                else{
                    map.Add(new XAttribute("role", from));
                }
            }
            //generate roles by suffixes and prefixes
            foreach (var roles in result.Elements("roles")){
                var prefixes = roles.attr("code").split();
                var suffixes = roles.attr("name").split();
                foreach (var prefix in prefixes){
                    foreach (var suffix in suffixes){
                        result.Add(new XElement("role",
                                                new XAttribute("name", prefix + "_" + suffix)
                                       ));
                    }
                }
                roles.Remove();
            }

            //generate roles by suffixes and prefixes
            foreach (var maps in result.Elements("maps")){
                var regex = maps.attr("code");
                var replace = maps.attr("name");
                foreach (var prefix in regex){
                    foreach (var role in result.Elements("role")){
                        var name = role.attr("name");
                        var newname = name.replace(regex, replace);
                        if (newname != name){
                            result.Add(new XElement("map",
                                                    new XAttribute("role", name),
                                                    new XAttribute("as", newname)
                                           ));
                        }
                    }
                }
                maps.Remove();
            }

            //generate roles by suffixes and prefixes
            foreach (var admin in result.Elements("admin")){
                var name = admin.attr("code");
                result.Add(new XElement("map",
                                        new XAttribute("user", name.Replace("/", "\\")),
                                        new XAttribute("as", "ADMIN")
                               ));
                admin.Remove();
            }
        }

	    /// <summary>
	    /// 	An index of object
	    /// </summary>
	    public int Idx { get; set; }

	    /// <summary>
	    /// 	Вызывается при вызове Reset
	    /// </summary>
	    /// <param name="data"> </param>
	    /// <returns> любой объект - будет включен в состав результатов <see cref="ResetEventResult" /> </returns>
	    /// <remarks>
	    /// 	При использовании стандартной настройки из <see cref="ServiceBase" /> не требует фильтрации опций,
	    /// 	настраивается на основе атрибута <see cref="RequireResetAttribute" />
	    /// </remarks>
	    public object Reset(ResetEventData data) {
		    reload();
		    return null;
	    }

	    /// <summary>
	    /// 	Возващает объект, описывающий состояние до очистки
	    /// </summary>
	    /// <returns> </returns>
	    public object GetPreResetInfo() {
		    return null;
	    }
    }
}