using System.Collections.Generic;

namespace Comdiv.Extensibility {
    public interface IRegistryLoader {
        void Load(IDictionary<string, object> registry);
        
    }
}