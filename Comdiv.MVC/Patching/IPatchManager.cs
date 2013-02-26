using System.Collections.Generic;
using System.Linq;
using Comdiv.Distribution;
using Comdiv.IO;
using Comdiv.Patching;

namespace Comdiv.MVC.Patching{
    public interface IPatchManager : IPackageRepository,IReloadAble{
        IFilePathResolver TargetFileSystem { get; set; }
        IEnumerable<IMvcPatch> Patches { get; }
        void ApplyStartupPathces();
        void SetInstalledMark(IMvcPatch patch);
        bool IsInstalled(IMvcPatch patch);
        void Install(string patchCode);
        void DropInstallationMarks();
    }
}