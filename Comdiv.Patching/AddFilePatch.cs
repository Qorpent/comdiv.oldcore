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
using System.Collections.Generic;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Patching{
    public class AddFilePatch : DefaultPatch{
        public AddFilePatch(){
            Root = "usr/";
        }

        public string Root { get; set; }
        public string Files { get; set; }

        public override IList<IPackageInstallTask> Tasks{
            get{
                if (base.Tasks.Count == 0){
                    foreach (var file in Files.split()){
                        base.Tasks.Add(new FileTask{TargetPath = Root + file, Content = Manager.PathResolver.Read(file)});
                    }
                }
                return base.Tasks;
            }
        }
    }
}