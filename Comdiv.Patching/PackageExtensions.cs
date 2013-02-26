using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Patching{
    public static class PackageExtensions{
        public static bool DependOn(this IPackage package, IPackage otherPackage, IPackageRepository repository){
            var deps = package.GetDependences();
            foreach (var dep in deps){
                if (dep.Name == otherPackage.Identity.Name){
                    return true;
                }
                var p = repository.Load(dep.Name);
                if (p.DependOn(otherPackage, repository)){
                    return true;
                }
            }
            return false;
        }
    }
}