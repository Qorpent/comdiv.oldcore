using System.Reflection;

namespace Comdiv.Extensibility.Boo.Dsl
{
    public interface ICompiler
    {
        Assembly Compile(ICompileDescription description);
    }
}
