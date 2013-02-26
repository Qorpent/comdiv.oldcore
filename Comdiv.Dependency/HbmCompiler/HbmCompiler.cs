//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.IO;
using Comdiv.Extensions;
using FluentNHibernate;

namespace Comdiv.HbmCompiler {
    public class HbmCompiler {
        public void Execute(Type type, string outdir, Action<string> writelog = null) {
            CleanUpDirectory(outdir);
            writelog = writelog ?? (s => { });
            var model = type.create<PersistenceModel>();
            model.WriteMappingsTo(outdir);
            writelog("model " + type.Name + " wrote to " + outdir);
        }

        private void CleanUpDirectory(string outdir) {
            Directory.CreateDirectory(outdir);
            foreach (var file in Directory.GetFiles(outdir, "*.hbm.xml")) {
                File.Delete(file);
            }
        }
    }
}