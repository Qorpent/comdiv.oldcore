using System.Collections.Generic;
using Boo.Lang.Compiler;

namespace Comdiv.Extensibility.Boo.Dsl{
    public class CompileDescription : ICompileDescription{
        public string AssemblyName { get; set; }

        public string SaveAssemblyPath { get; set; }

        public IEnumerable<IAssemblyProvider> CompileModifierAssemblies { get; set; }

        public IEnumerable<IAssemblyProvider> ReferenceAssemblies { get; set; }

        public IEnumerable<ISourceProvider> Sources { get; set; }

        public CompilerPipeline Pipeline{ get; set;}
        }
    }
