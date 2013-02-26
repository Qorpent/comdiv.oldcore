using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Extensions;

namespace Comdiv.Extensibility{
    public class BooDSLRunner
    {
        private const string extractReferencesRegex = @"\#ref\s+(?<a>[^\r\n]+)";
        private IDictionary<string, object> globals;

        public BooDSLRunner() {}

        public BooDSLRunner(string file){
            BooScript = BooEval.LoadScript(file);
            if (Path.IsPathRooted(file))
                Environment.CurrentDirectory = Path.GetDirectoryName(file);
        }

        public string BooScript { get; set; }

        public IDictionary<string, object> Globals{
            get { return globals ?? (globals = new Dictionary<string, object>()); }
        }

        public CompilerErrorCollection Run(params string[] args){
            var referencedAssemblies = ExtractAssemblies();
            var pipeline = ExtractPipeline() ?? new CompileToFile();

            var parameters = new CompilerParameters(true);

            //parameters.Ducky = true;

            parameters.LibPaths.Add(Environment.CurrentDirectory);
            parameters.LibPaths.Add(Path.Combine(Environment.CurrentDirectory, "bin"));
            parameters.OutputAssembly = "script.dll";

            foreach (var assembly in referencedAssemblies){
                parameters.AddAssembly(assembly);
                parameters.References.Add(assembly);
            }
            parameters.Pipeline = pipeline;
            var compiler = new BooCompiler(parameters);
            parameters.Input.Add(new ReaderInput("script", new StringReader(BooScript)));
            var result = compiler.Run();

            if (result.Errors.yes()) return result.Errors;

            var resultAssembly = result.GeneratedAssembly;


            var global__ = resultAssembly.getTypesWithAttribute<GlobalsHandlingClassAttribute>();
            var global = global__.no() ? null : global__.FirstOrDefault();


            if (global.yes() && Globals.Count != 0){
                foreach (var pair in Globals){
                    var pi = global.GetProperty(pair.Key, BindingFlags.Static | BindingFlags.Public | BindingFlags.SetProperty);
                    if (null == pi) throw new Exception("Нет статического свойства с именем " + pair.Key);
                    pi.SetValue(null, pair.Key, null);
                }
            }

            resultAssembly.EntryPoint.Invoke(null, new[]{args});

            return null;
        }

        private CompilerPipeline ExtractPipeline(){
            var reference = Regex.Match(BooScript, @"\#pipe\s+(?<t>[\S]+)").Groups["t"].Value;
            if (reference.yes()) return reference.toType().create<CompilerPipeline>();
            return null;
        }

        public IEnumerable<Assembly> ExtractAssemblies(){
            return BooScript.findAll(extractReferencesRegex).Select(m =>{
                                   var aref = Environment.ExpandEnvironmentVariables(m.Groups["a"].Value);
                                   var aname = aref;
                                   if (aref.Contains(".dll"))
                                       aname = Path.GetFileNameWithoutExtension(aref);
                                   var name = new AssemblyName(aname);
                                   if (aref != aname) name.CodeBase = aref;
                                   return AppDomain.CurrentDomain.Load(name);
                                   /*
				           		if(aname.True())
				           		{
				           			try
				           			{
				           				return AppDomain.CurrentDomain.Load(aname);
				           			}
				           			catch (Exception)
				           			{
				           				
				           			}
				           		}
				           		return Assembly.LoadFrom(aref);*/
                });
        }
    }

    public class GlobalsHandlingClassAttribute : Attribute {}
}