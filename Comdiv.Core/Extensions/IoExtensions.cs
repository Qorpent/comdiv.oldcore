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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Extensions
{
    //TODO: разобрать что лишнее и откомментить
    ///<summary>
    ///</summary>
    public static partial class IoExtensions
    {
        public static string addpath(this string basis, string rel){
            return Path.Combine(basis, rel);
        }

        public static string GetDirectoryAndFile(string path) {
            return Path.Combine(Path.GetFileName(Path.GetDirectoryName(path)),Path.GetFileName(path));
        }

        public static string ComputeHash(string[] filenames) {
            var result = "";
            foreach (var filename in filenames) {
                if(String.IsNullOrWhiteSpace(filename)) continue;
                if(!File.Exists(filename)) continue;
                var info = new FileInfo(filename);
                result += filename + info.LastWriteTime.ToString("yyyyMMddhhmmssffffff");
            }
    
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(result)));
        }

        public static FileLevel GetLevel(string path) {
            if(path.Contains("/sys/")) {
                return FileLevel.sys;
            }else if(path.Contains("/mod/")) {
                return FileLevel.mod;
            }
            else if (path.Contains("/usr/")) {
                return FileLevel.usr;
            }
            return FileLevel.None;
        }

        public static string mapPath(this string path)
        {

            
            return mapPath(path,(Func<string, string>)null);
            
        }
        public static string clearPath(this string path){
            return path.Replace("\\", "/").Replace("//", "/");
        }

        private static Func<string, string> getRootMaper(){
            if (HttpContext.Current != null || CurrentHttp!=null) return getByContext;
            //if (myapp.conversation.Current!=null && myapp.conversation.Current.Data.ContainsKey("context")){
            //    return getByConversationContext;
            //}
            return getByEnvironment;
        }

        [ThreadStatic]public static HttpContext CurrentHttp;

        private static string getByEnvironment(string path){
            path = path.Replace("~/", String.Empty);
            //если нет, то адаптируем путь из веба в обычный

            return Path.Combine(Environment.CurrentDirectory, path);
        }
        [NoCover("cannot cover it well because i cannot normally setup http context in test environment")]
        private static string getByContext(string path){
            
            if (path.StartsWith("~"))
            {
                string localPath = path.Substring(2);
                string globalPath = (HttpContext.Current ?? CurrentHttp).Server.MapPath("~\\");
                return Path.GetFullPath(Path.Combine(globalPath, localPath));
            }
            return HttpContext.Current.Server.MapPath(path);
        }

      

        public static string normalizePath(this string path){
            return path.replace(@"\\", "/").replace("/+", "/");
        }

        public static bool isSubpathOf(this string path, string root)
        {
            return Path.GetFullPath(path).ToUpper().StartsWith(Path.GetFullPath(root).ToUpper());
        }

        private static string mapPath(this string path, string root){
            return mapPath(path, p=>Path.GetFullPath(Path.Combine(root,p)));
        }

        public static string mapPath(this string path, Func<string, string> rootMapper)
        {
            //path = path.Replace("file:///", "");
            rootMapper = rootMapper ?? getRootMaper();
            path = Environment.ExpandEnvironmentVariables(path);
            //всегда сначала надо вскрыть переменные

            if (Path.IsPathRooted(path)) return Path.GetFullPath(path);
            //если он корневой - вернуть сразу

            return Path.GetFullPath( rootMapper(path)).clearPath();
            
        }

        public static string readResource(this Assembly assembly, string resource)
        {
            try
            {
                using (Stream s = assembly.GetManifestResourceStream(resource))
                {
                    using (var sr = new StreamReader(s, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("cannot find {0},{1}", assembly.GetName().Name, resource), ex);
            }
        }

        public static FileInfo toFileInfo(this string filePath)
        {
            if (filePath.no()) return null;
            return new FileInfo(filePath);
        }

        public static DirectoryInfo toDirInfo(this string dirPath)
        {
            if (dirPath.no()) return null;
            return new DirectoryInfo(dirPath);
        }

        public static bool hasNewer(this FileInfo file, string pattern)
        {
            if (file.no()) return false;
            return
                file.Directory.GetFiles(pattern, SearchOption.AllDirectories).FirstOrDefault(
                    f => f.LastWriteTime > file.LastWriteTime) != null;
        }

        public static string prepareTemporaryDirectory(this string folderName){
            return prepareTemporaryDirectory(folderName, true);
        }

        public static string prepareTemporaryDirectory(this string folderName,bool clean)
        {
            string path = Path.Combine(Path.GetTempPath(),folderName);
            Directory.CreateDirectory(path);
            if (clean){
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}