using System;
using System.Linq;
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
    public interface ICustomFilterDecscriptorProvider : IFilterDescriptorProvider{
        bool IsMatch(Type type);
    }
}