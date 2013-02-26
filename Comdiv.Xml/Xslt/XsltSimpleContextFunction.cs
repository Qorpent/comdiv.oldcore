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
#region

using System;
using System.Collections;
using System.Reflection;
using System.Xml.XPath;
using System.Xml.Xsl;


#endregion

namespace Comdiv.Xslt{

    #region

    #endregion

    /// <summary>
    /// Summary description for XsltSimpleContextFunction.
    /// </summary>
    public class XsltSimpleContextFunction : IXsltContextFunction{
        private readonly object helper;
        private readonly string name;

        public XsltSimpleContextFunction(object helper_, string name_){
            helper = helper_;
            name = name_;
            buildArgs();
        }

// ReSharper disable UnusedPrivateMember

        #region IXsltContextFunction Members

        public XPathResultType[] ArgTypes { get; private set; }

        public XPathResultType ReturnType{
            get { return XPathResultType.Any; }
        }

        public int Minargs{
            get { return 0; }
        }

        public int Maxargs{
            get { return 100; }
        }

        public object Invoke(XsltContext xsltContext, object[] _args, XPathNavigator docContext){
            return helper.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, helper, _args);
        }

        #endregion

        

        private void buildArgs(){
            var res = new ArrayList();
            for (var i = 0; i < 100; i++)
                res.Add(XPathResultType.Any);
            ArgTypes = res.ToArray(typeof (XPathResultType)) as XPathResultType[];
        }
    }
}