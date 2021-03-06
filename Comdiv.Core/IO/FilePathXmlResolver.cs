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
using System.Net;
using System.Xml;
using Comdiv.Design;

namespace Comdiv.IO{
    public class FilePathXmlResolver : XmlResolver{
        public FilePathXmlResolver(IFilePathResolver targetSystem){
            TargetSystem = targetSystem;
        }

        private IFilePathResolver TargetSystem { get; set; }
        [NoCover]
        public override ICredentials Credentials{
            [NoCover]
            set { }
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri){
            if (relativeUri == String.Empty){
                return new Uri("this://doc/");
            }

            return new Uri("fs:///" + relativeUri);
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn){
            if (absoluteUri.ToString() == "this://doc/"){
                return new MemoryStream(TargetSystem.ReadBinary(".this"));
            }

            var path = absoluteUri.AbsolutePath.Substring(1); //trim leading "/"

            return new MemoryStream(TargetSystem.ReadBinary(path));
        }
    }
}