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

namespace Comdiv.Security.Acl{
    public class AclRuleEntity : AclRule, IEntityDataPattern{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }

        #region IEntityDataPattern Members

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime Version { get; set; }

        public virtual string RuleTypeString {
            get { return RuleType.ToString(); }
            set { this.RuleType = (AclRuleType) Enum.Parse(typeof (AclRuleType), value, true); }
        }

        #endregion
    }
}