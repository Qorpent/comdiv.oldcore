// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
// COMDIV:
//  1. dependency MvcContrib dropped (imported namespaces)

using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Boo.Lang.Extensions;
using Comdiv.Extensibility;
using Comdiv.Extensibility.Brail;

//using Microsoft.Web.Mvc;


namespace MvcContrib.Comdiv.ViewEngines.Brail{

    public class MvcViewEngineOptions : ViewEngineOptions {
        public MvcViewEngineOptions() {
            this.ViewEngineType = "MvcContrib.Comdiv.ViewEngines.Brail.BooViewEngine";
            this.BaseType = "MvcContrib.Comdiv.ViewEngines.Brail.BrailBase";
            AssembliesToReference.Add(typeof (MvcViewEngineOptions).Assembly); //Brail's assembly
            AssembliesToReference.Add(typeof (Controller).Assembly); //MVC Framework's assembly
            AssembliesToReference.Add(typeof (AssertMacro).Assembly); //Boo.Lang.Extensions assembly
            AssembliesToReference.Add(typeof (RouteValueDictionary).Assembly); //Microsoft.Mvc
            AssembliesToReference.Add(typeof (IncludeastMacro).Assembly);
            NamespacesToImport.Add("MvcContrib.Comdiv.ViewEngines.Brail");
            NamespacesToImport.Add("System.Web.Mvc");
            NamespacesToImport.Add("System.Web.Mvc.Html.LinkExtensions");
            NamespacesToImport.Add("System.Web.Routing");
            NamespacesToImport.Add("System.Web.Mvc.Html");
        }
    }

}