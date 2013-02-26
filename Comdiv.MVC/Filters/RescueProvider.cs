using System;
using System.Linq;
using System.Reflection;
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
using Comdiv.MVC.Controllers;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Filters{
    public class RescueProvider : IRescueDescriptorProvider{
        private readonly IRescueDescriptorProvider defaultprovider = new DefaultRescueDescriptorProvider();

        #region IRescueDescriptorProvider Members

        public void Service(IMonoRailServices serviceProvider){
            defaultprovider.Service(serviceProvider);
        }

        public RescueDescriptor[] CollectRescues(Type memberInfo){
            if (memberInfo == typeof (ErrorsController)){
                return new RescueDescriptor[]{};
            }
            var result = defaultprovider.CollectRescues(memberInfo).ToList();
            result.Add(new RescueDescriptor(typeof (ErrorsController), typeof (ErrorsController).GetMethod("Rescue"),
                                            typeof (Exception)));
            return result.ToArray();
        }

        public RescueDescriptor[] CollectRescues(MethodInfo memberInfo){
            if (memberInfo.DeclaringType == typeof (ErrorsController)){
                return new RescueDescriptor[]{};
            }
            var result = defaultprovider.CollectRescues(memberInfo).ToList();
            result.Add(new RescueDescriptor(typeof (ErrorsController), typeof (ErrorsController).GetMethod("Rescue"),
                                            typeof (Exception)));
            return result.ToArray();
        }

        #endregion
    }
}