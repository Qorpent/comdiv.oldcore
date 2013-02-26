using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public enum AccessLevel{
        None = 0,
        Root = 1, //only roots can acess this controller or action
        Admin = 2, //only roots/admins can acess this controller or action
        Public = 4, //no restrictions can be applied
        AdminAlways = 8, //admins - no restrictions
    }

    /// <summary>
    /// Связыает дополнительный JS/CSS с контроллером/действием
    /// </summary>
    public class PkgAttribute : Attribute{
        public string Name { get; set; }
        public PkgAttribute(string name){
            this.Name = name;
        }
        public static string[] Get(IControllerContext context){
			if(null==context.ControllerDescriptor)return new string[]{};
            var action = context.ControllerDescriptor.Actions[context.Action] as MethodInfo;
            if(null==action)return new string[]{};
            var result = new List<string>();
            var type = action.DeclaringType;
            var typeattr = type.GetCustomAttributes(typeof (PkgAttribute), true).Cast<PkgAttribute>();
            foreach (var t in typeattr){
                result.Add(t.Name);
            }
            var actattr = action.GetCustomAttributes(typeof (PkgAttribute), true).Cast<PkgAttribute>();
            foreach (var t in actattr){
                result.Add(t.Name);
            }
            return result.ToArray();
        }
    }

    public class AccessLevelAttribute : Attribute{
        protected readonly AccessLevel level;

        public AccessLevelAttribute(AccessLevel level){
            this.level = level;
        }

        public AccessLevel Level{
            get { return level; }
        }
    }

    public class PublicAttribute : AccessLevelAttribute{
        public PublicAttribute() : base(AccessLevel.Public) {}
    }

    public class AdminAttribute : AccessLevelAttribute{
        public AdminAttribute() : base(AccessLevel.Admin) {}
    }

    public class RootAttribute : AccessLevelAttribute{
        public RootAttribute() : base(AccessLevel.Root) {}
    }

    public class AdminAlwaysAttribute : AccessLevelAttribute{
        public AdminAlwaysAttribute() : base(AccessLevel.AdminAlways) {}
    }
}