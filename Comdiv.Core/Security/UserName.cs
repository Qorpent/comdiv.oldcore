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
using System.Security.Principal;

namespace Comdiv.Security{
    /// <summary>
    ///   Parses usual windows identity names and split it into
    ///   domain and name slots and defines if it's local
    ///   identity
    /// </summary>
    public sealed class UserName{
        public UserName(string username){
            username = username.Replace("/", "\\").ToLower();
            FullName = username;
            if (username.Contains("\\")){
                Domain = username.Split('\\')[0];
                Name = username.Split('\\')[1];
                if (Domain == Environment.MachineName.ToLower()){
                    IsLocal = true;
                }
            }
            else{
                Name = username;
                Domain = Environment.MachineName;
                FullName = (Domain + "\\" + Name).ToLower();
                IsLocal = true;
            }
        }
        public string LocalizedName{
            get{
                if(IsLocal){
                    return "local\\" + Name;
                }else{
                    return FullName;
                }
            }
        }
        public static UserName For(IPrincipal principal){
            return For(principal.Identity.Name);
        }
        public static UserName For(IIdentity identity){
            return For(identity.Name);
        }
        public static UserName For(string principal){
            return new UserName(principal);
        }
        public string FullName { get; private set; }
        public string Domain { get; private set; }
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }
    }
}