using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Patching{
    public class PackageOrderer : IComparer<IPackage>{
        private readonly IPackageRepository repository;

        public PackageOrderer(IPackageRepository repository){
            this.repository = repository;
        }

        #region IComparer<IPackage> Members

        public int Compare(IPackage x, IPackage y){
            if (x.DependOn(y, repository)){
                return 1;
            }
            if (y.DependOn(x, repository)){
                return -1;
            }
            return x.GetDependences().Count().CompareTo(y.GetDependences().Count());
            //return 0;
        }

        #endregion
    }
}