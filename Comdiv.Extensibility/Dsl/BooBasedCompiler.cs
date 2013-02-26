using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.Extensibility.Boo.Dsl{
    public class BooBasedCompiler:ICompiler{
        public static BooBasedCompiler CreateForDsl(){
            var result = new BooBasedCompiler();
            result.InternalCompiler.Parameters.WhiteSpaceAgnostic = true;
            result.InternalCompiler.Parameters.Ducky = true;
            return result;
        }
        private BooCompiler compiler = new BooCompiler();

        public BooCompiler InternalCompiler{
            get { return compiler; 
            }
        }

        public IList<Assembly> DefaultReferences { get; set; }

        public CompilerContext LastCompiledContext { get; set; }

        public Assembly Compile(ICompileDescription description){
            compiler.Parameters.References.Clear();
            compiler.Parameters.Pipeline = description.Pipeline ?? new CompileToFile();
            compiler.Parameters.LoadDefaultReferences();
            compiler.Parameters.Input.Clear();
            if(DefaultReferences.yes()){
                foreach (var assembly in DefaultReferences){
                    compiler.Parameters.References.Add(assembly);
                }
            }
            compiler.Parameters.References.Add(typeof(String).Assembly);
            if (description.ReferenceAssemblies.yes()){
                foreach (var assemblyProvider in description.ReferenceAssemblies){
                    compiler.Parameters.References.Add(assemblyProvider.GetAssembly());
                }
            }

            if(description.CompileModifierAssemblies.yes()){
                foreach (var assemblyProvider in description.CompileModifierAssemblies){
                    assemblyProvider.GetAssembly().GetTypes()
                        .Where(
                        t=>
                            typeof(IBooCompilerPipelineModifier).IsAssignableFrom(t)
                            && typeof(IBooCompilerPipelineModifier)!=t)
                        .Select(
                        t=>
                            t.create<IBooCompilerPipelineModifier>())
                        .OrderBy(
                        m=>m.Order
                        )
                        .map(
                        m=>
                            m.Modify(compiler.Parameters.Pipeline));
                }
            }
            compiler.Parameters.OutputType = CompilerOutputType.Library;
            if (description.SaveAssemblyPath.hasContent()){
                compiler.Parameters.GenerateInMemory = false;
                compiler.Parameters.OutputAssembly = Path.Combine(description.SaveAssemblyPath,
                                                                  description.AssemblyName + ".dll");
            }else{
                compiler.Parameters.GenerateInMemory = true;
                compiler.Parameters.OutputAssembly = description.AssemblyName + ".dll";
            }
            
            foreach (var sourceProvider in description.Sources){
                if(sourceProvider is IBooSourceProvider){
                    compiler.Parameters.Input.Add(((IBooSourceProvider)sourceProvider).GetInput());
                }else{
                    var name = "module" + Guid.NewGuid();
                    if(sourceProvider is IWithName){
                        name = ((IWithName) sourceProvider).Name;
                    }
                    compiler.Parameters.Input.Add(new ReaderInput(name,new StringReader(sourceProvider.GetSource())));
                    
                }
            }
            var result = compiler.Run();
            LastCompiledContext = result;
            if(result.GeneratedAssemblyFileName.hasContent()){
                return Assembly.LoadFrom(result.GeneratedAssemblyFileName);
            }
            if(null==result.GeneratedAssembly)throw new CompilerError("null generation");
            return result.GeneratedAssembly;
        }
    }
}