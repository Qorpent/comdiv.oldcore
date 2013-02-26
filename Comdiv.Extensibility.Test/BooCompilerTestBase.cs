using System;
using System.IO;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;

namespace Comdiv.Extensibility.Test {
    public class BooCompilerTestBase {
        public CompilerContext Compile(string name, string code, CompilerPipeline pipeline = null, Action<CompilerParameters> prepare = null) {
            if(null==pipeline) pipeline = new Parse();
            var compiler = new BooCompiler();
            compiler.Parameters.Pipeline = pipeline;
            compiler.Parameters.Input.Add(new ReaderInput(name, new StringReader(code)));
            if(prepare!=null) {
                prepare(compiler.Parameters);
            }
            return compiler.Run();
        }
    }
}