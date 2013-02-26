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
    public class ExternalPartitionsFilter : Filter{
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
        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            var partitions = Container.get<IPartitionsSource>().GetPartitions(controller);
            ((Controller) controller).PropertyBag["partitions"] = partitions;
            foreach (var partition in partitions){
                partition.Execute(controller);
            }
        }
    }
}