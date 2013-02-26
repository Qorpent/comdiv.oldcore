using System.Text;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Pipelines;
using Comdiv.Extensibility.ExtensionsCompiler;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    public class ExtensionsPreprocessorTest : ExtensionsPreprocessorTestBase {
        protected override Module postProcess() {
            
            return new ExtensionsPreprocessor().ConvertModule(cu.CompileUnit.Modules[0], new CompilerContext());          
        }

        protected override CompilerPipeline getPipeline() {
            return new Parse();
        }
    }
}
