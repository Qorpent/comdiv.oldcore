using System.Collections.Generic;
using System.Linq;
using Comdiv.Distribution;
using Comdiv.Model;using Comdiv.Patching; using Comdiv.Logging;
using Comdiv.Model.Interfaces;

namespace Comdiv.MVC.Patching{
    public interface IMvcPatch : IPackage, IWithCode,IWithComment{
        bool AutoLoad { get; set; }
        IPatchManager Manager { get; set; }
        bool IsInstalled { get; }
        bool ReInstallable { get; set; }
    }
}