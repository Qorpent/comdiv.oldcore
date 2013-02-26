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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Comdiv.IO;

namespace Comdiv.Patching{
    public class AutoXsltTask : TaskBase{
        public bool TreatFileNotExistedAsError { get; set; }
        public Action<XsltArgumentList> ArgPreparator { get; set; }

        public override IPackageInstallResult Do(IPackage package, IFilePathResolver target){
            var wasError = false;
            var result = new DefaultPackageInstallResult{Message = "applying auto xslt"};
            var allfiles = package.PathResolver.ResolveAll("", "*.*");
            var xsltTaskFiles = allfiles.Where(s => s.EndsWith(".taskinfo.xslt"));
            xsltTaskFiles.Select(
                s =>{
                    if (wasError){
                        return s;
                    }

                    var targetFileName = s.Substring(0, s.Length - ".taskinfo.xslt".Length);

                    if (GetProceed(target, targetFileName, result)){
                        var xsltSource = package.PathResolver.ReadBinary(s);
                        var targetSource = target.Read(targetFileName);
                        var sw = new StringWriter();
                        var xws = new XmlWriterSettings();
                        xws.Indent = true;
                        xws.NewLineHandling = NewLineHandling.Entitize;
                        xws.OmitXmlDeclaration = true;
                        xws.Encoding = Encoding.UTF8;
                        var xw = XmlWriter.Create(sw, xws);
                        var xsltTransform = new XslCompiledTransform();
                        var sets = new XsltSettings(true, true);

                        var doc = XmlReader.Create(new StringReader(targetSource));
                        var xslt = XmlReader.Create(new MemoryStream(xsltSource));
                        var xmlResolver = new FilePathXmlResolver(package.PathResolver);
                        xsltTransform.Load(xslt, sets, xmlResolver);
                        var args = new XsltArgumentList();
                        if (ArgPreparator != null){
                            ArgPreparator(args);
                        }
                        xsltTransform.Transform(doc, args, xw, xmlResolver);
                        target.Write(targetFileName, sw.ToString());
                    }
                    result.SubMessages.Add("PROCESSED: " + targetFileName);
                    Console.WriteLine("PROCESSED: " + targetFileName);
                    return s;
                }
                ).ToList();
            return result;
        }

        private bool GetProceed(IFilePathResolver target, string targetFileName, DefaultPackageInstallResult result){
            var proceed = true;
            if (!target.Exists(targetFileName)){
                if (TreatFileNotExistedAsError){
                    result.State = ResultState.Error;
                    result.SubMessages.Add("ERROR/NOFILE:" + targetFileName);
                    proceed = false;
                }
                else{
                    result.SubMessages.Add("NOFILE:" + targetFileName);
                    proceed = false;
                }
            }
            return proceed;
        }
    }
}