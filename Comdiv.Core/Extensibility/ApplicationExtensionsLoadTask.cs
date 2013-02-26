using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.IO;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    public class ApplicationExtensionsLoadTask {
        private ExtensionsFileSystemProvider lfs;
        private Process process;
        private DateTime lastLoad;
        private Assembly cachedAssembly;
        private string tmp;

        public ApplicationExtensionsLoadTask(string exepath=null,bool web = true, string dllname=null) {
            cleanupOldDlls();
            this.lfs = new ExtensionsFileSystemProvider(null, web, dllname);
            this.tmp = myapp.files.Resolve("~/tmp/lastextcompile.txt", false);
            myapp.files.Write("~/tmp/lastextcompile.txt", "");
            var apppath =Path.Combine( (exepath ?? ""), "extensionscompiler.exe");
            var commandargs = web ? "--web true" : "--web false";
            if(!string.IsNullOrWhiteSpace(dllname)) {
                commandargs += " --dllname " + dllname;
            }
            commandargs += " --root \""+myapp.files.Resolve("~/",false) + "\"";
            commandargs += " --outfile \"" + tmp + "\"";
            var pi = new ProcessStartInfo(apppath, commandargs);
            pi.UseShellExecute = true;
            this.process = new Process();
            process.StartInfo = pi;
            
        }

        private void cleanupOldDlls() {
            var path = myapp.files.Resolve("~/tmp/extensionlibcache/", false);
            if (!Directory.Exists(path)) return;
            var subdirs = Directory.GetDirectories(path);
            if(subdirs.Length==0)return;
            subdirs = subdirs.OrderByDescending(x => new DirectoryInfo(x).LastAccessTime).Skip(1).ToArray();
            foreach (var subdir in subdirs) {
                try {
                    Directory.Delete(subdir,true);
                }catch(IOException) { //единственная возможная ошибка - ошибка доступа, мы ее проглатываем - не зачистили и не зачистили
                    
                }catch(UnauthorizedAccessException ) {
                    
                }
            }
        }

        /// <summary>
        /// Запускает компилятор на выполнение
        /// </summary>
        public void Start() {
            process.Start();
        }
        /// <summary>
        /// Завершает выполнение компилятора, если все ОК - возвращает откомилированную сборку
        /// иначе 
        /// </summary>
        /// <returns></returns>
        public Assembly Finish() {
            bool finished = process.WaitForExit(360000);
            var output = File.ReadAllText(tmp);
            
            if(!finished) {
                throw new ApplicationExtensionsLoaderException("timeout");
            }
            if(process.ExitCode!=0) {
                
                throw new ApplicationExtensionsLoaderException(){ErrorCode = process.ExitCode,OutputLog = output};
            }
            if(lastLoad<lfs.GetLibraryVersion()) {
                cachedAssembly = lfs.LoadAssembly();
                lastLoad = lfs.GetLibraryVersion();
            }
            return cachedAssembly;
        }
    }
}
