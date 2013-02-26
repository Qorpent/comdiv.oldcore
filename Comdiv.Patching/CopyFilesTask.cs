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
using System.Text.RegularExpressions;
using Comdiv.IO;

namespace Comdiv.Patching{
    public abstract class TaskBase : IPackageInstallTask{
        private string name;

        public string RootDirectory { get; set; }

        #region IPackageInstallTask Members

        public string Name{
            get { return name ?? (name = GetType().Name); }
            set { name = value; }
        }

        public abstract IPackageInstallResult Do(IPackage package, IFilePathResolver target);

        #endregion
    }

    public class CopyFilesTask : TaskBase{
        private readonly IDictionary<string, CopyBehaviour> masks = new Dictionary<string, CopyBehaviour>();

        public IDictionary<string, CopyBehaviour> Masks{
            get { return masks; }
        }

        public override IPackageInstallResult Do(IPackage package, IFilePathResolver target){
            var result = new DefaultPackageInstallResult{Message = ToString()};
            package.PathResolver.ResolveAll("")
                .Where(s => !(GetBehaviour(s) == CopyBehaviour.Skip))
                .Select(s =>{
                            target.Resolve("~/" + s, false);
                            return s;
                        })
                .ToList();
            foreach (var file in package.PathResolver.ResolveAll("")){
                var behaviour = GetBehaviour(file);
                switch (behaviour){
                    case CopyBehaviour.Skip:
                        result.SubMessages.Add(string.Format("SKIP:{0}", file));
                        //   Console.WriteLine(string.Format("SKIP:{0}", file));
                        break;

                    case CopyBehaviour.None:
                        goto default;

                    case CopyBehaviour.OverWrite:
                        goto default;

                    case CopyBehaviour.NewOnly:
                        if (!target.Exists(file)){
                            goto default;
                        }
                        result.SubMessages.Add(string.Format("EXISTS:{0}", file));
                        Console.WriteLine(string.Format("EXISTS:{0}", file));
                        break;

                    default:
                        target.Write(file, package.PathResolver.ReadBinary(file));
                        result.SubMessages.Add(string.Format("COPY:{0}", file));
                        // Console.WriteLine(string.Format("COPY:{0}", file));
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the behaviour.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public CopyBehaviour GetBehaviour(string file){
            var behaviour = CopyBehaviour.None;
            foreach (var mask in Masks){
                if (Regex.IsMatch(file, mask.Key, RegexOptions.Compiled)){
                    behaviour = mask.Value;
                    break;
                }
            }
            if (behaviour == CopyBehaviour.None){
                behaviour = CopyBehaviour.OverWrite;
            }
            return behaviour;
        }

        public override string ToString(){
            return string.Format("{0}, {1}, {2}", Name, RootDirectory,
                                 Masks.Select(p => p.Key + "=>" + p.Value + ";").Aggregate((s1, s2) => s1 + " " + s2));
        }

        public static CopyFilesTask CopyAll(){
            return new CopyFilesTask{Name = "copy all", RootDirectory = "/"};
        }

        public static CopyFilesTask Default(){
            var result = new CopyFilesTask{Name = "default copy", RootDirectory = "/"};
            result.masks["/?[^/]+\\.taskinfo"] = CopyBehaviour.Skip;
            result.masks["\\.svn(/|$)"] = CopyBehaviour.Skip;
            result.Masks["^/?usr(/|$)"] = CopyBehaviour.NewOnly;
            result.Masks["^/?sys(/|$)"] = CopyBehaviour.NewOnly;
            result.Masks["^/?\\.pkg(/|$)"] = CopyBehaviour.Skip;
            result.Masks["^/?\\.tmp(/|$)"] = CopyBehaviour.Skip;

            return result;
        }
    }
}