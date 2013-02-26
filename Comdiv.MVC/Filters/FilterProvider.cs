using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Descriptors;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Security;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Filters{
    public class FilterProvider : FilterDescriptorProviderCollection{
        protected override IEnumerable<FilterDescriptor> internalCollectFilters(Type type,
                                                                                IEnumerable<FilterDescriptor> current){
            current = current.Union(base.internalCollectFilters(type, current));

            //HACK: ErrorsController находится на особом положении
            if (type == typeof (ErrorsController)){
                return current;
            }
            var defaults = new List<FilterDescriptor>();

            Action<Type, ExecuteWhen, int> inject =
                (t, when, order) =>{
                    if (null == current.FirstOrDefault(f => t.IsAssignableFrom(f.FilterType))){
                        defaults.Add(new FilterDescriptor(t, when, order, null));
                    }
                };

            //всем остальным впрыскиваем стандартный набор
            inject(typeof (AuthorizeFilter), ExecuteWhen.BeforeAction, -1000);
            inject(typeof (LayoutPrepareFilter), ExecuteWhen.AfterAction, -1000);
            inject(typeof (WorkspacePrepareFilter), ExecuteWhen.AfterAction, -500);
            inject(typeof (ConversationFilter), ExecuteWhen.Always, -10000);
            inject(typeof (LogFilter), ExecuteWhen.Always, 1000);
#if !OPTIMIZED
            
            inject(typeof (ScriptedFilter), ExecuteWhen.Always, -1000);
#endif
            inject(typeof (RedirectViewFilter), ExecuteWhen.Always, 500);


            var excludes = type.GetCustomAttributes(typeof (ExcludeFilterAttribute), true)
                .Cast<ExcludeFilterAttribute>().Select(f => f.Type).Distinct().ToArray();


            return
                defaults.Where(d => null == current.FirstOrDefault(c => c.FilterType == d.FilterType)).Union(current).
                    Where(
                    f => null == excludes.FirstOrDefault(t => t.IsAssignableFrom(f.FilterType)));
        }
    }
}