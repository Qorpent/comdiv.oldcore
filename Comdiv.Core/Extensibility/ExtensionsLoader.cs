using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    /// <summary>
    /// Собственно загружает расширения из указанной библиотеки
    /// </summary>
    public class ExtensionsLoader {
        public IDictionary<string ,object > GetRegistry(Assembly assembly) {
            var registry = new Dictionary<string, object>();
            var types = assembly.GetTypes();
            foreach (var type in types.OrderBy(x=>x.Name)) {
                if(typeof(IRegistryLoader).IsAssignableFrom(type)) {
                    var loader = (IRegistryLoader)type.create();
                    loader.Load(registry);
                }
            }
            myapp.ExtensionsAssembly = assembly;
            myapp.ExtensionsRegistry = registry;
            return registry;
        }
    }
}
