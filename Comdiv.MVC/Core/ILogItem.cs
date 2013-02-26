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

namespace Comdiv.MVC{
    public interface ILogItem : IWithId, IWithVersion{
        DateTime Time { get; set; }
        string Area { get; set; }
        string Controller { get; set; }
        string Action { get; set; }
        string Params { get; set; }
        string Event { get; set; }
        string CustomData { get; set; }
        string Result { get; set; }
        string Usr { get; set; }
        DateTime RequestTime { get; set; }
    }
}