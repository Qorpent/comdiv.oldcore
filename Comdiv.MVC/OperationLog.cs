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
    public class OperationLog : IOperationLog{
        public OperationLog(){
            Range = new DateRange(DateExtensions.Begin, DateExtensions.End);
        }

        #region IOperationLog Members

        public virtual int Id { get; set; }
        public virtual int Elapsed { get; set; }
        public virtual DateRange Range { get; set; }
        public virtual string Usr { get; set; }
        public virtual string ObjectType { get; set; }
        public virtual string ObjectCode { get; set; }
        public virtual string OperationType { get; set; }
        public virtual string System { get; set; }
        public virtual string Url { get; set; }
        public virtual string Xml { get; set; }

        #endregion
    }
}