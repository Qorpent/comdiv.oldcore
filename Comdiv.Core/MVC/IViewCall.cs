using System.Collections.Generic;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.MVC{
    public interface IViewCall
    {
        string View { get; set; }
        IDictionary<string, object> Parameters { get; }
        int Idx { get; set; }
        string Text { get; set; }
    }
}