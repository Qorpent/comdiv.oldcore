using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework.Descriptors;
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
    public class FilterDescriptorProviderCollection : AbstractExtendedFilterDescriptorProvider{
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
        protected override IEnumerable<FilterDescriptor> internalCollectFilters(Type type,
                                                                                IEnumerable<FilterDescriptor> current){
            foreach (
                var customFilterDecscriptorProvider in
                    Container.all<ICustomFilterDecscriptorProvider>().Where(f => f.IsMatch(type))){
                foreach (var filterDescriptor in customFilterDecscriptorProvider.CollectFilters(type)){
                    yield return filterDescriptor;
                }
            }
        }
    }
}