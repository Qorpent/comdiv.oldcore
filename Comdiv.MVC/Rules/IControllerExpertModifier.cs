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

namespace Comdiv.MVC.Rules{
    public interface IControllerExpertModifier : IWithIdx{
        bool IsMatch(string key, IControllerExpert expert);
        IControllerExpert Modify(IControllerExpert expert);
    }

    public abstract class AbstractControllerExpertModifier : IControllerExpertModifier{
        public string ExpertList { get; set; }

        #region IControllerExpertModifier Members

        public virtual bool IsMatch(string key, IControllerExpert expert){
            return ExpertList.split().Contains(key);
        }

        public IControllerExpert Modify(IControllerExpert expert){
            internalModify(expert);
            return expert;
        }

        public int Idx { get; set; }

        #endregion

        protected abstract void internalModify(IControllerExpert expert);
    }
}