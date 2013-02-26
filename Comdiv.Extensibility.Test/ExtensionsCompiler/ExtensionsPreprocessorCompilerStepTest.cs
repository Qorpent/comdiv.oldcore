using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Extensibility.ExtensionsCompiler;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ExtensionsPreprocessorCompilerStepTest : ExtensionsPreprocessorTestBase {
        protected override Module postProcess() {
            return srcmodule;
        }

        protected override CompilerPipeline getPipeline() {
            return ExtensionsPreprocessorCompilerStep.Extend( new Parse());
        }
    }
}