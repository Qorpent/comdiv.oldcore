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
using System.Reflection;
using System.Text;
using System.Threading;
using Comdiv.Design;
using Comdiv.Extensions;

namespace Comdiv.IO{
    ///<summary>
    ///</summary>
    public static class FilePathResolverExtensions{

        public static string ResolveAsLocal(this IFilePathResolver resolver, string path){
            if (!(resolver is ILayeredFilePathResolver)) throw new Exception("cannot crop");
            var rpath = resolver.Resolve(path);
            if(null==rpath) return null;
            var root = (resolver as ILayeredFilePathResolver).Resolve("~/",false);
            var result = rpath.Replace(root, "");
            if (result.StartsWith("/")) result = result.Substring(1);
            return result.Replace("\\","/");
        }

        public static string[] ResolveAllAsLocal(this IFilePathResolver resolver, string path, string mask){
            return ResolveAllAsLocal(resolver, path, mask, true);
        }

        public static string[] ResolveAllAsLocal(this IFilePathResolver resolver, string path, string mask, bool recursive)
        {
            if (!(resolver is ILayeredFilePathResolver)) throw new Exception("cannot crop");
            var files = resolver.ResolveAll(path,mask,true,recursive);
            var root = (resolver as ILayeredFilePathResolver).Resolve("~/", false);
            return files.Select(f =>{
                                    var result = f.Replace(root, "");
                                    if (result.StartsWith("/")) result = result.Substring(1);
                                    result = result.Replace("\\", "/");
                                    return result;
                                }).ToArray();
        }

        public static T Delete<T>(this T resolver, string path) where T : IFilePathResolver
        {
            var resolved = resolver.Resolve(path);
            if(null!=resolved){
                Thread.Sleep(20);
                File.Delete(resolved);
            }
            resolver.Reload();
            return resolver;
        }

        public static T DeleteDirectory<T>(this T resolver, string path) where T : IFilePathResolver
        {
            var resolved = resolver.Resolve(path,false);
            if(Directory.Exists(resolved)){
                Directory.Delete(resolved,true);
            }
            return resolver;
        }
        public static bool Exists(this IFilePathResolver resolver, string path) 
        {
            return null!= resolver.Resolve(path);
        }
        public static void DefinePseudoFile(this IFilePathResolver resolver, string name, string content){
            resolver.Data["__file__" + name] = true;
            resolver.Write("~/__pseudo_files/"+name,content);
        }

        public static string Read(this IFilePathResolver resolver, string path){
            return Read(resolver, path, Encoding.UTF8);
        }

        public static string Read(this IFilePathResolver resolver, string path, Encoding encoding){
            if(resolver.Data.ContainsKey("__file__"+path)){
                return resolver.Read("~/__pseudo_files/" + path);
            }
            var globalpath = resolver.Resolve(path, true);
            if (null == globalpath) return String.Empty;
            return File.ReadAllText(globalpath, encoding);
        }


        public static string ReadConcat(this IFilePathResolver resolver,string path){
            string result = "";
            foreach (var all in ResolveAll(resolver,path)){
                result += resolver.Read(all) + Environment.NewLine;
            }
            return result;
        }

    

        public static byte[] ReadBinary(this IFilePathResolver resolver, string path)
        {
            if (resolver.Data.ContainsKey("__file__" + path))
            {
                return resolver.ReadBinary("~/__pseudo_files/" + path);
            }
            var globalpath = resolver.Resolve(path, true);
            if (null == globalpath) return new byte[]{};
            return File.ReadAllBytes(globalpath);
        }

        public static void Write(this IFilePathResolver resolver, string path){
            Write(resolver, path, String.Empty);
        }

        public static void Write(this IFilePathResolver resolver, string path, string content){
            Write(resolver, path, content, Encoding.UTF8);
        }
        [Worker]
        [Hack]
        public static void Write(this IFilePathResolver resolver, string path, string content, Encoding encoding){
            var globalpath = resolver.Resolve(path, false);
            Directory.CreateDirectory(Path.GetDirectoryName(globalpath));
            File.WriteAllText(globalpath, content, encoding);
            //HACK: there are some strange things on some systems, it's better to get pause after writing on disk
            resolver.Reload();
            Thread.Sleep(10);
        }

        public static string Resolve(this IFilePathResolver resolver, string path){
            return resolver.Resolve(path, true);
        }

        public static DateTime LastWriteTime(this IFilePathResolver resolver,string path, string  searchmask = "*.*") {
            var files = ResolveAll(resolver, path, searchmask);
            if(files==null||files.Count()==0) return DateExtensions.Begin;
            return 
                (from f in files
                 let d = File.GetLastWriteTime(f)
                 select d).Max()
                ;
        }

        public static IEnumerable<string> ResolveAll(this IFilePathResolver resolver, string path)
        {
            return resolver.ResolveAll(path, null, true);
        }
        public static IEnumerable<string> ResolveAll(this IFilePathResolver resolver, string path, bool existedOnly)
        {
            return resolver.ResolveAll(path, null, existedOnly);
        }
        public static IEnumerable<string> ResolveAll(this IFilePathResolver resolver,string path, string searchMask){
            return resolver.ResolveAll(path, searchMask, true);
        }

        public static IEnumerable<string> ResolveAllCompressed(this IFilePathResolver resolver, string path)
        {
            return resolver.ResolveAll(path, "*.*", true).Where(x => !x.Contains(".svn")).CompressDuplicatedFiles();
        }
        
        public static IEnumerable<string> ResolveAllCompressed(this IFilePathResolver resolver, string path, string searchMask)
        {
            return resolver.ResolveAll(path, searchMask, true).Where(x=>!x.Contains(".svn")).CompressDuplicatedFiles();
        }

        
        private static IEnumerable<string> CompressDuplicatedFiles(this IEnumerable<string> files){
            IList<string> processed = new List<string>();
            foreach (var file in files){
                var name = Path.GetFileName(file);
                if(!processed.Contains(name)){
                    processed.Add(name);
                    yield return file;
                }
            }
        }
        [Worker]
        [Hack]
        public static void Write(this IFilePathResolver resolver, string path, byte[] bytes){
            path = resolver.Resolve(path,false);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path,bytes);
            resolver.Reload();
            Thread.Sleep(10);

        }

        public static string GetDefaultRoot()
        {
            var codebase = Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "");
            var foldername = Path.GetFileName(Path.GetDirectoryName(codebase)).ToLower();
            bool isbin = foldername == "bin";
            bool isext = !isbin && Path.GetFileName(Path.GetDirectoryName(foldername)).ToLower() == "extensions";
            if (isbin) return Path.GetDirectoryName(Path.GetDirectoryName(codebase)); //bin forlder mode - move up level
            if (isext) return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(codebase))); //extensions mode forlder mode - move up  2 level
            return Path.GetDirectoryName(codebase); //native mode - no web context detected, single application
        }
    }
}