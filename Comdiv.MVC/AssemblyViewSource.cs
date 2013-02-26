using System.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
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
    public class AssemblyViewSource : IMvcStarter{
        public string Source { get; set; }

        public string AssemblyName{
            get { return Source.split()[1]; }
        }

        public string Namespace{
            get { return Source.split()[0]; }
        }

        #region IMvcStarter Members

        public void Init(){
            MonoRailConfiguration.GetConfig().ViewEngineConfig.AssemblySources.Add(
                new AssemblySourceInfo(AssemblyName, Namespace));
        }

        #endregion
    }
}