using System.Collections.Generic;
using Comdiv.Extensions;

namespace Comdiv.Persistence {
    public class ParamMappingHelper {
        public IDictionary<string ,object > GetParameters(IParametersProvider provider) {
            if(provider.UseCustomMapping) {
                return provider.GetParameters();
            }
            var props = provider.GetType().GetProperties();
            var result = new Dictionary<string, object>();
            foreach (var pi in props) {
                var a = pi.getFirstAttribute<ParamAttribute>();
                if(null!=a) {
                    var name = pi.Name;
                    if (a.Name.hasContent()) name = a.Name;
                    name = "@" + name;
                    result[name] = pi.GetValue(provider,null);
                }
            }
            return result;
        }
    }
}