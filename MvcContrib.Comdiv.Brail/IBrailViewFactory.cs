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
using System.Web.Mvc;

namespace MvcContrib.Comdiv
{
    public interface IBrailViewFactory : IViewEngine
    {
        event ResolveViewNameEventHandler OnResolveViewName;
        event ResolveBrailCodeEventHandler OnResolveCode;
    }

    public class ResolveViewNameEventArgs : EventArgs
    {
        public ResolveViewNameEventArgs(string basis, string name)
        {
            Basis = basis;
            Name = name;
        }
        public string Basis { get; protected set; }
        public string Name { get; protected set; }
        public string Result { get; set; }
    }

    public class ResolveBrailCodeEventArgs : EventArgs
    {
        public ResolveBrailCodeEventArgs(string name)
        {

            Name = name;
        }
        public string Name { get; protected set; }
        public string Result { get; set; }
    }

    public delegate void ResolveViewNameEventHandler(object sender, ResolveViewNameEventArgs args);
    public delegate void ResolveBrailCodeEventHandler(object sender, ResolveBrailCodeEventArgs args);
}
