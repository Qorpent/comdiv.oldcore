using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Boo.Dsl{
    public class DirectoryBasedDynamicBooAssemblyProvider:IAssemblyProvider{
        public string AssemblyName { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string LibPath { get; set; }
        private bool useFileBasedCache = true;
        public bool UseFileBasedCache{
            get { return useFileBasedCache; }
            set { useFileBasedCache = value; }
        }
        protected IEnumerable<Assembly> getReferences(){
            ResolveEventHandler assemblyResolver =
                (o, a) => {
                              var assemblyPath = Path.Combine(LibPath, a.Name + ".dll");
                              if(!File.Exists(assemblyPath)) return null;
                              return Assembly.LoadFrom(assemblyPath);
                };
            AppDomain.CurrentDomain.AssemblyResolve+=assemblyResolver;
            try
            {
                var filename = Path.Combine(InputDirectory, "ref.boo.xml");
                if (!File.Exists(filename)) yield break;
                foreach (XPathNavigator assemblyname in new XPathDocument( filename).CreateNavigator().Select("//ref"))
                {
                    var aname = assemblyname.Value;
                    yield return AppDomain.CurrentDomain.Load(aname);
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolver;
            }

        }
        public Assembly GetAssembly(){
            var assemblyFile = Path.Combine(OutputDirectory ?? InputDirectory, AssemblyName + ".dll");
            bool useCached = true;
            if(!File.Exists(assemblyFile)) useCached = false;
            else{
                var checkDate = new FileInfo( assemblyFile).LastWriteTime;
                foreach (var file in new DirectoryInfo( InputDirectory).GetFiles("*.boo*",SearchOption.AllDirectories)){
                    if(file.LastWriteTime > checkDate){
                        useCached = false;
                        break;
                        
                    }
                }
            }
            if(useCached){
                return loadAsCached(assemblyFile);
            }
            else{
                return compile(assemblyFile);
            }
        }

        private BooBasedCompiler myCompiler = BooBasedCompiler.CreateForDsl();
        private Assembly compile(string file){
            var descriptor = new CompileDescription();
            descriptor.AssemblyName = this.AssemblyName;
            descriptor.SaveAssemblyPath = OutputDirectory ?? InputDirectory;
            var assemblies = getReferences().Select(a=> (IAssemblyProvider)new SimpleAssemblyProvider(a)).ToList();
            descriptor.CompileModifierAssemblies = assemblies;
            descriptor.ReferenceAssemblies = assemblies;
            descriptor.Sources =
               new DirectoryInfo( InputDirectory).GetFiles("*.boo*", SearchOption.AllDirectories).Select(
                    f =>(ISourceProvider) new FileBasedBooSourceProvider(f.FullName));
            return myCompiler.Compile(descriptor);
        }

        private Assembly loadAsCached(string file){
            var alreadyloaded = AppDomain.CurrentDomain.GetAssemblies().LastOrDefault(a => a.GetName().Name == AssemblyName);
            if (alreadyloaded.no()){
                return Assembly.LoadFrom(file);
            }
            return alreadyloaded;
        }
    }
}