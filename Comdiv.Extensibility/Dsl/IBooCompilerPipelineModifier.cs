using Boo.Lang.Compiler;

namespace Comdiv.Extensibility.Boo.Dsl{
    public interface IBooCompilerPipelineModifier:IOrdered{
        void Modify(CompilerPipeline pipeline);
    }
}