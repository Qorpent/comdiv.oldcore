using System.Reflection;

namespace Comdiv.Extensibility.Boo.Dsl{
    public class SimpleAssemblyProvider : IAssemblyProvider{

        private readonly Assembly myAssembly;
        public SimpleAssemblyProvider(Assembly myAssembly) {
            this.myAssembly = myAssembly;
        }

        public Assembly GetAssembly(){
            return myAssembly;
        }
    }
}