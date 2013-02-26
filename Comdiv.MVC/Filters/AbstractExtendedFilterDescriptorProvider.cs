using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Descriptors;
using Castle.MonoRail.Framework.Providers;
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

namespace Comdiv.MVC.Filters{
    public abstract class AbstractExtendedFilterDescriptorProvider : IFilterDescriptorProvider{
        private IFilterDescriptorProvider defaultProvider;

        protected IFilterDescriptorProvider DefaultProvider{
            get { return defaultProvider ?? (defaultProvider = new DefaultFilterDescriptorProvider()); }
        }

        #region IFilterDescriptorProvider Members

        public FilterDescriptor[] CollectFilters(Type controllerType){
            var defaultDescriptors = defaultProvider.CollectFilters(controllerType);
            var myDescriptors = internalCollectFilters(controllerType, defaultDescriptors);
            return myDescriptors.Union(defaultDescriptors).Distinct().ToArray();
        }

        public void Service(IMonoRailServices serviceProvider){
            DefaultProvider.Service(serviceProvider);
            internalService(serviceProvider);
        }

        #endregion

        protected abstract IEnumerable<FilterDescriptor> internalCollectFilters(Type type,
                                                                                IEnumerable<FilterDescriptor> current);

        protected virtual void internalService(IMonoRailServices provider) {}
    }
}