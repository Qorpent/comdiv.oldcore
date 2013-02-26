

using Boo.Lang.Compiler;

namespace Comdiv.Extensibility.Boo.Dsl{
    public interface IBooSourceProvider:ISourceProvider{
        ICompilerInput GetInput();
    }
}