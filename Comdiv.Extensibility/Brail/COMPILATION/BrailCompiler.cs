using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.Steps;
using Comdiv.Application;

namespace Comdiv.Extensibility.Brail
{
	public class BrailCompiler : ViewCompiler {
        private BooCompiler compiler;

        public bool StopOnError { get; set; }

        public override Assembly Compile(ViewCompilerInfo info) {
			lock(this) {
				var log = myapp.QorpentApplication.LogManager.GetLog(this.GetType().FullName + ";MvcHandler", this);
				try {
					if (AllInMemory) info.InMemory = true;
					initCompiler(info);
					setupPipeline(compiler, info);
					setupParameters(compiler, info);
					setupSources(compiler, info);
					var cunit = new CompileUnit();
					cunit["sources"] = info.Sources;
					var result = compiler.Run(cunit);
					this.LastResult = result;
					if (result.Errors.Count != 0 && !info.ProcessingTest) {
						throw new Exception(result.Errors.ToString(true));
					}
					if (!info.ProcessingTest) {
						return result.GeneratedAssembly;
					}
					return null;
				}catch(Exception ex) {
					log.Error("",new BrailCompilerException(info,ex),this);
					throw;
				}
			}

        }

        public CompilerContext LastResult { get; set; }

        public override void SetOutput(TextWriter outwrtiter) {
	        lock (this) {
		        initCompiler(null);
		        this.compiler.Parameters.OutputWriter = outwrtiter;
	        }
        }

        public bool TracePipelineSteps { get; set; }

        public bool AllInMemory  { get; set; }

        void Pipeline_AfterStep(object sender, CompilerStepEventArgs args)
        {
            if(TracePipelineSteps)this.compiler.Parameters.OutputWriter.WriteLine("after: "+args.Step.GetType().Name);
        }

        void Pipeline_BeforeStep(object sender, CompilerStepEventArgs args)
        {
            if(TracePipelineSteps)this.compiler.Parameters.OutputWriter.WriteLine("before: " + args.Step.GetType().Name);
        }

        private void initCompiler(ViewCompilerInfo info) {
            if(null==this.compiler) {
                this.compiler = new BooCompiler();
                setupAssemblies(compiler, info);
            }
        }

        private void setupSources(BooCompiler compiler, ViewCompilerInfo info) {
            compiler.Parameters.Input.Clear();
            foreach (var source in info.Sources) {
                var input = new StringInput(source.Key, source.GetContent());
                compiler.Parameters.Input.Add(input);
            }
        }

        private void setupAssemblies(BooCompiler compiler, ViewCompilerInfo info) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if(assembly.IsDynamic) continue;
				if(!compiler.Parameters.References.Contains(assembly)) {
					compiler.Parameters.References.Add(assembly);
				}
            }
        }

        private void setupParameters(BooCompiler compiler, ViewCompilerInfo info) {
            compiler.Parameters.GenerateInMemory = info.InMemory;
            compiler.Parameters.Ducky = true;
            compiler.Parameters.Debug = true;
            compiler.Parameters.OutputType = CompilerOutputType.Library;
            if(info.InMemory) {
                compiler.Parameters.OutputAssembly = info.AssemblyName + ".dll";
            }else {
                compiler.Parameters.OutputAssembly = Path.Combine(info.TargetDirecrtory, info.AssemblyName + ".dll");
            }
        }

        private void setupPipeline(BooCompiler compiler, ViewCompilerInfo info) {
            if(info.InMemory) {
                if (info.ProcessingTest) {
                    compiler.Parameters.Pipeline = new ResolveExpressions();
                }
                else {
                    compiler.Parameters.Pipeline = new CompileToMemory();
                }
            }else {
                
                compiler.Parameters.Pipeline = new CompileToFile();
            }
            compiler.Parameters.Pipeline.RemoveAt(0);
            compiler.Parameters.Pipeline.Insert(0,new WSAIgnoranceParsingStep());
            var processor = new BrailPreProcessor(null, true);
            compiler.Parameters.Pipeline.Insert(0, processor);
            compiler.Parameters.Pipeline.InsertAfter(typeof(WSAIgnoranceParsingStep), new ExpandBmlStep());
            compiler.Parameters.Pipeline.InsertAfter(typeof(WSAIgnoranceParsingStep), new IncludeAstMacroExpandStep());
            
            compiler.Parameters.Pipeline.Insert(2, new TransformToBrailStep(info.Options));
            compiler.Parameters.Pipeline.InsertAfter(typeof(TransformToBrailStep), new BrailRenamerAndTimeStamper());
            compiler.Parameters.Pipeline.InsertAfter(typeof(ExpandBmlStep),
                                                     new InterpolationUnescapeStep());
            compiler.Parameters.Pipeline.InsertAfter(typeof (MacroAndAttributeExpansion), new OutputWriteUnification());

           // if (!info.BrailProcessingTest) {
                compiler.Parameters.Pipeline.Replace(typeof (ProcessMethodBodiesWithDuckTyping),
                                                     new ReplaceUknownWithParameters());
                if (!info.ProcessingTest) {
                    compiler.Parameters.Pipeline.Replace(typeof (ExpandDuckTypedExpressions),
                                                         new ExpandDuckTypedExpressions_WorkaroundForDuplicateVirtualMethods
                                                             ());
                }

#if !LIB2
            compiler.Parameters.Pipeline.Replace(typeof(InitializeTypeSystemServices),
                                                 new InitializeCustomTypeSystem());
#endif
                compiler.Parameters.Pipeline.InsertBefore(typeof (MacroAndAttributeExpansion),
                                                          new FixTryGetParameterConditionalChecks());
                compiler.Parameters.Pipeline.RemoveAt(
                    compiler.Parameters.Pipeline.Find(typeof (IntroduceGlobalNamespaces)));
            //}
            if(null!=PreparePipeline) {
                PreparePipeline(compiler.Parameters.Pipeline);
            }
            if(StopOnError)this.compiler.Parameters.Pipeline.BreakOnErrors = true;
            this.compiler.Parameters.Pipeline.BeforeStep += Pipeline_BeforeStep;
            this.compiler.Parameters.Pipeline.AfterStep += Pipeline_AfterStep;
        }

        public event Action<CompilerPipeline> PreparePipeline;
    }
}
