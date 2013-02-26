using System;
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

namespace Comdiv.MVC.Core{
    public abstract class DynamicActionHandler : IDynamicAction, IDynamicActionHandler{
        private Type controllerType;

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
        #region IDynamicAction Members

        public abstract object Execute(IEngineContext engineContext, IController controller,
                                       IControllerContext controllerContext);

        #endregion

        #region IDynamicActionHandler Members

        public string ControllerTypeName { get; set; }

        public Type ControllerType{
            get{
                if (controllerType.no() && ControllerTypeName.hasContent()){
                    controllerType = ControllerTypeName.toType();
                }
                return controllerType;
            }
            set{
                controllerType = value;
                ControllerTypeName = value.AssemblyQualifiedName;
            }
        }

        public virtual bool Match(IController controller){
            return controller.GetType() == ControllerType;
        }

        public string ActionName { get; set; }

        public IDynamicAction Action{
            get { return this; }
        }

        #endregion
    }
}