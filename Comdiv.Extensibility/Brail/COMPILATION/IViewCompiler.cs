using System;
using System.IO;
using System.Reflection;

namespace Comdiv.Extensibility.Brail {
    public interface IViewCompiler {
        Type CompileSingle(ViewCodeSource source, ViewEngineOptions options);
        Assembly CompileApplication(ViewEngineOptions options);
        Assembly Compile(ViewCompilerInfo info);
        void SetOutput(TextWriter outwrtiter);
        IViewSourceResolver Resolver { get; set; }
    }
}