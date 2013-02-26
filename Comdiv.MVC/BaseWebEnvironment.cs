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

namespace Comdiv.Cfg{
    public class BaseWebEnvironment : IWebEnvironment{
        #region IWebEnvironment Members

        public virtual string GetHomePage(){
            return Commons.DefaultHomePage;
        }

        public virtual string GetPrintVersionCheckboxPrefix(){
            return Commons.DefaultPrintVersionCheckboxPrefix;
        }

        public virtual char GetDelimiter(){
            return Commons.DefaultDelimiter;
        }

        public virtual string GetUserDataDirectory(){
            return Commons.DefaultUserData;
        }

        #endregion
    }
}