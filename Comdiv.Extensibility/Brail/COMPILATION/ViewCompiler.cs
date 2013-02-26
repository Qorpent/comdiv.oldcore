using System;
using System.IO;
using System.Reflection;

namespace Comdiv.Extensibility.Brail {
    public abstract class ViewCompiler : IViewCompiler {
        public Type CompileSingle(ViewCodeSource source, ViewEngineOptions options) {
            var info = new ViewCompilerInfo
                           {
                               AssemblyName = Guid.NewGuid().ToString(),
                               InMemory = true,
                               Sources = new[] {source},
                               Options =  options,
                           };
            var assembly = this.Compile(info);
            return assembly.GetType(source.Key.Replace("/","_0_"));
        }

        public Assembly CompileApplication(ViewEngineOptions options) {
            var resolver = (BrailSourceResolver)Resolver ?? new BrailSourceResolver();
            var sources = resolver.GetAll();
            var assemblyname = "_application_views";
            var targetdirectory = resolver.FileSystem.Resolve("~/tmp/", false);
            var info = new ViewCompilerInfo
                           {
                               AssemblyName = assemblyname,
                               Sources = sources,
                               TargetDirecrtory = targetdirectory,
                               InMemory = false,
                               Options =  options,
                           };
            if(!info.InMemory) {
                Directory.CreateDirectory(targetdirectory);
            }
            return Compile(info);
        }

        public abstract Assembly Compile(ViewCompilerInfo info);
        public abstract void SetOutput(TextWriter outwrtiter);

        public IViewSourceResolver Resolver { get; set; }
    }
}