using System;
using System.Linq;
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
    public class ActionDescriptionAttribute : Attribute{
        public ActionDescriptionAttribute(ActionRole actionRole, ActionSeverity actionSeverity, string description){
            ActionRole = actionRole;
            ActionSeverity = actionSeverity;
            Description = description;
        }

        public ActionDescriptionAttribute(string description){
            Description = description;
        }

        public string Description { get; set; }
        public ActionRole ActionRole { get; set; }
        public ActionSeverity ActionSeverity { get; set; }
    }

    public enum ActionRole{
        Unknown,
        Public,
        User,
        Admin,
        Developer
    }

    [Flags]
    public enum ActionSeverity{
        Unknown,
        NonCritical,
        AppHang,
        DataAccess,
        DataLoss,
        SecurityLack,
        Dangerous
    }
}