using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Security{
    public class MvcTokenProvider : IAclTokenResolverImpl{
        #region IAclTokenResolverImpl Members

        public string GetToken(object aclTarget){
            if (!(aclTarget is IMvcContext)){
                return null;
            }
            return GetToken((IMvcContext) aclTarget);
        }

        public string GetToken(Type aclType, string aclId){
            if (!(typeof (IMvcContext).IsAssignableFrom(aclType))){
                return null;
            }
            throw new NotSupportedException("cannot provide token for abstract MVC context");
        }

        public int Idx { get; set; }

        #endregion

        [Extensible(typeof (IAclTokenRewriter))]
        public string GetToken(IMvcContext context){
            //default token for mvc is path to action 
            var result = string.Format("/app/{0}/{1}/{2}", context.Area, context.Name, context.Action);
            //resolving empty 'folders'
            return result.Replace("//", "/").ToLower();
        }
    }
}