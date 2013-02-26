using System.Collections.Generic;

namespace Comdiv.Persistence {
    public interface IParametersProvider {
        bool UseCustomMapping { get; }
        IDictionary<string, object> GetParameters();
    }
}