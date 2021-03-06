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
    public interface IDynamicActionHandler : IControllerAwareItem{
        string ActionName { get; }
        IDynamicAction Action { get; }
        string ControllerTypeName { get; set; }
        Type ControllerType { get; set; }
    }
}