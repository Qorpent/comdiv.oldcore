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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ExtensionsFileSystemProvider {
        public ExtensionsFileSystemProvider(string root = null, bool web = false, string  dllname = null) {
            DllName = dllname ?? "extensions";
            Web = web;
            if (!string.IsNullOrWhiteSpace(root)) {
                Root = root;
                _files = new DefaultFilePathResolver(root);
            }
            else {
                Root = files.Resolve("~/", false);
            }
            DllRoot = Root;
			LoaderExceptions = new List<Exception>();
           // if (Web) DllRoot = Path.Combine(DllRoot, @".\bin\");
            Console.WriteLine(DllName);
            this.WriteLog = s => { };
        }

        public string DllRoot { get; set; }

        public string Root { get; set; }
        public bool Web { get; set; }

        protected IFilePathResolver files {
            get { return _files ?? (_files = myapp.files); }
        }

        public bool IsNeedRecompile() {
            return GetLibraryVersion() < GetSourceVersion();
        }

        private string[] cached;
        private string root;
        private IFilePathResolver _files;

        public string[] GetFileNames() {
            if (null == cached) {
                WriteLog("try resolve files");
                var list = new List<string>(files.ResolveAll("extensions", "*.boo", true,s=> WriteLog("\tnfs:: "+s)));
                var filteredlist = new Dictionary<string, string>();
                foreach (var fname in list) {
                    WriteLog("prepare " + fname);
                    var key = IoExtensions.GetDirectoryAndFile(fname);
                    if (!filteredlist.ContainsKey(key)) {
                        filteredlist[key] = fname;
                    }
                }
                cached = filteredlist.Values.ToArray();
            }
            return cached;
        }

        public DateTime GetSourceVersion() {
            var hashfile = files.Resolve(GetHashPath(), false);
            var hash = evalHash();
            if (File.Exists(hashfile)) {
                var existedhash = File.ReadAllText(hashfile);
                if (existedhash == hash) {
                    return new FileInfo(hashfile).LastWriteTime;
                }
            }

            storeHash(hash);
            return new FileInfo(hashfile).LastWriteTime;
        }

        public string GetHashPath() {
            return "~{0}/ExtensionFilesSource_hash"._format(Web ? "/tmp" : "");
        }

        private string evalHash() {
            return IoExtensions.ComputeHash(GetFileNames());
        }

        private void storeHash(string hash) {
            files.Write(GetHashPath(), hash);
        }

        public DateTime GetLibraryVersion() {
            var file = files.Resolve(getDllPath(), false);
            if (File.Exists(file)) {
                return new FileInfo(file).LastWriteTime;
            }
            return DateExtensions.Begin;
        }

        public string DllName { get; set; }

        public Action<string> WriteLog { get; set; }

        private string getDllPath() {
            return "~{0}/{1}.dll"._format(Web ? "/tmp" : "",DllName);
        }

        public string GetLibraryPath() {
            return files.Resolve(getDllPath(), false);
        }

		public IList<Exception> LoaderExceptions { get; private set; } 

        public Assembly[] GetReferencedAssemblies(bool throwonloaderror = false) {
           
            if(!Directory.Exists(DllRoot)) {
                return new Assembly[]{};
            }

            var existed = new List<string>();
                var result = new List<Assembly>();
                foreach (var dll in Directory.GetFiles(DllRoot, "*.dll",SearchOption.AllDirectories)) {
                    if (dll.ToLower().Replace("\\", "/").Contains("/extensionslib/")) continue;
                    if (dll.ToLower().Replace("\\", "/").Contains("/tmp/")) continue;
                    if(Path.GetFileName(dll)==Path.GetFileName(DllName)) continue;
                    if(!existed.Contains(Path.GetFileNameWithoutExtension(dll))){
						try {
							result.Add(ReflectionExtensions.LoadAssemblyFromFile(dll, DllRoot, new[] {"/extensions/"}));
							existed.Add(Path.GetFileNameWithoutExtension(dll));
						}catch(TypeLoadException ex) {
							if (throwonloaderror) throw;
							LoaderExceptions.Add(ex);
						}catch(BadImageFormatException ex) {
							if (throwonloaderror) throw;
							LoaderExceptions.Add(ex);
						}
                    }
                }
                return result.ToArray();
         
          
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyPath = Path.Combine(DllRoot, args.Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            return Assembly.LoadFrom(assemblyPath);
        }

        public Assembly LoadAssembly() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            try {
                var path =
                    files.Resolve(
                        "~/tmp/extensionlibcache/" + GetLibraryVersion().ToString("yyyyMMddHHmmssffff"), false);
                Directory.CreateDirectory(path);
                var fname = Path.GetFileName(GetLibraryPath());
                    fname = Path.Combine(path, fname);
                if (!File.Exists(fname)) {
                    File.Copy(GetLibraryPath(), fname);
                }
                return Assembly.LoadFile(fname);
            }
            finally {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }
    }
}