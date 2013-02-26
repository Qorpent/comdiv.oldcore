using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Controllers{
    public class DefaultPartitionsSource : IPartitionsSource{
        #region IPartitionsSource Members

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public IEnumerable<IPartition> GetPartitions(object controller){
            lock (this){
                var mvc = MvcContext.Create((Controller) controller);
                return Container.all<IPartition>().Where(p => p.IsMatch(mvc)).OrderBy(p => p.Idx).ToArray();
            }
        }

        public void Apply(object controller){
            lock (this){


                var c = controller as Controller;
                var partitions = GetPartitions(controller);
                c.PropertyBag["partitions"] = partitions;
                foreach (var partition in partitions){
                    partition.Execute(controller);
                }
            }
        }

        #endregion
    }
}