using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;

using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Booxml{
    public class Substitution{
        public string From { get; set; }
        public string To { get; set; }
        public string As { get; set; }
        public bool Elements { get; set; }
        public bool UseFirst { get; set; }
    }
}