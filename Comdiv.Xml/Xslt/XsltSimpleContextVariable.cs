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
using System.Xml.XPath;
using System.Xml.Xsl;

using Comdiv.Transformation;

#endregion

namespace Comdiv.Xslt{

    #region

    #endregion

    #region

    #endregion

    /// <summary>
    /// Summary description for XsltSimpleContextVariable.
    /// </summary>
    /// 
    [Obsolete]
    public class XsltSimpleContextVariable : IXsltContextVariable{
        private readonly object ext;

        private readonly string name;

        private readonly ITransformator tr;

        public XsltSimpleContextVariable(string name_, ITransformator transformator) {
            tr = transformator;
            name = name_;
        }

        public XsltSimpleContextVariable(object ext_, string name_){
            ext = ext_;
            name = name_;
        }

        #region IXsltContextVariable Members

        public bool IsLocal{
            get { return false; }
        }

        public XPathResultType VariableType{
            get { return XPathResultType.Any; }
        }

        public bool IsParam{
            get { return true; }
        }

        public object Evaluate(XsltContext xsltContext){
            object val = null;
            if (tr != null)
                val = tr.GetParameter(name);
            else
                val = ext.GetType().GetProperty(name).GetValue(ext, null);
            return val == null ? "" : val;
        }

        #endregion
    }
}