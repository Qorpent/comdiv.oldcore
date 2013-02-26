

namespace Comdiv.Extensibility.Boo.Dsl{
    public class SimpleSourceProvider : ISourceProvider{
        private readonly string source;
        public SimpleSourceProvider(string source) {
            this.source = source;
        }

        public string GetSource(){
            return this.source;
        }
    }
}