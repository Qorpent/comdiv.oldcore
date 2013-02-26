using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using Comdiv.MAS;

namespace mravc
{
    class Program
    {
        static void Main(string[] args)
        {
            new MonoRailAppViewCompiler().Run(args);
        }
    }


    public class MonoRailAppViewCompiler:MasConsoleApplication {
        private MONORAILBrailTypeFactory factory;
        private string[] test;

        public MonoRailAppViewCompiler() {
            this.CanIgnoreMas = true;
            IgnoreMasByDefault = true;
        }

        protected override void initialize()
        {
            base.initialize();
            IList<string> cache = new List<string>();
            foreach (var path in assemblyProbePaths.Union(new[] { myapp.files.Resolve("~/bin", true) }))
            {
                foreach (var file in Directory.GetFiles(path, "*.dll"))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    if (key.StartsWith("Comdiv") && !key.Contains("Model.Implementation") && !key.Contains("Comdiv.FluentView") && !cache.Contains(key))
                    {
                        Assembly.LoadFrom(file);
                        logtrace(Path.GetFileNameWithoutExtension(file));
                        cache.Add(key);
                    }
                }
            }
            
            this.factory = new MONORAILBrailTypeFactory(new BrailSourceResolver(), new MonoRailViewEngineOptions());
            factory.MyCompiler.SetOutput(Console.Out);
            factory.MyCompiler.StopOnError = Args.get("stoponerror","true").toBool();
            factory.MyCompiler.TracePipelineSteps = Args.get("tracepipeline","true").toBool();
            factory.Resolver.GetAllExcludes = Args.get("excludes", "").split().ToArray();
            this.test = Args.get("test", "").split().ToArray();
        }
        protected override void execute() {
            
           
            try {
                if (test.Length != 0) {
                    var info = new ViewCompilerInfo();
                    info.Sources = test.Select(x => factory.Resolver.GetFullInfo(x)).ToArray();
                    info.InMemory = true;
                    info.Options = factory.Options;
                    factory.MyCompiler.Compile(info);
                }else {
                    factory.CompileAll();
                }
            }catch(Exception e) {
                if(e.ToString().Contains("CompilerError")) {
                    logerror(e.Message);
                    
                }else {
                    throw;
                }
            }
        }
    }
}
