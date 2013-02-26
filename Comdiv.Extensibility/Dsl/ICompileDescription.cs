using System.Collections.Generic;
using Boo.Lang.Compiler;

namespace Comdiv.Extensibility.Boo.Dsl{
    public interface ICompileDescription{
        string AssemblyName { get; set; }
        string SaveAssemblyPath { get; set; }
        IEnumerable<IAssemblyProvider> CompileModifierAssemblies { get; }
        IEnumerable<IAssemblyProvider> ReferenceAssemblies { get; }
        IEnumerable<ISourceProvider> Sources { get; }
        CompilerPipeline Pipeline { get; set; }
    }
}