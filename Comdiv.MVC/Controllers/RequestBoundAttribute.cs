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
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RequestBoundAttribute : Attribute{
// This is a positional argument

        public RequestBoundAttribute() {}

        public RequestBoundAttribute(string paramName){
            ParamName = paramName;
        }

        public string ParamName { get; private set; }

        // This is a named argument
        public int NamedInt { get; set; }
    }
}