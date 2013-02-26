using System;

namespace Comdiv.Extensibility.Brail {
    public class ViewCompilerInfo {
        public ViewCompilerInfo() {
            InMemory = true;
            AssemblyName = Guid.NewGuid().ToString();
        }
        public bool InMemory { get; set; }
        public string AssemblyName { get; set; }
        public string TargetDirecrtory { get; set; }
        public ViewCodeSource[] Sources { get; set; }
        public ViewEngineOptions Options { get; set; }

        public bool ProcessingTest { get; set; }
    }
}