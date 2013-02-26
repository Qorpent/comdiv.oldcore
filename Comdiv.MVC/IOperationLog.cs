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
    public interface IOperationLog : IWithId, IWithRange{
        int Elapsed { get; set; }
        string Usr { get; set; }
        string ObjectType { get; set; }
        string ObjectCode { get; set; }
        string OperationType { get; set; }
        string System { get; set; }
        string Url { get; set; }
        string Xml { get; set; }
    }
}