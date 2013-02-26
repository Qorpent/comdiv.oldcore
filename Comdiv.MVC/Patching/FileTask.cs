using Comdiv.Distribution;
using Comdiv.IO;

namespace Comdiv.MVC.Patching{
    public class FileTask : IPackageInstallTask
    {
        public string TargetPath { get; set; }
        public string Content { get; set; }
        public bool Overwrite { get; set; }
        public string Name
        {
            get; set;
        }

        public IPackageInstallResult Do(IPackage package, IFileSystem target){
            if(Overwrite || !target.FileExists(TargetPath)){
                target.Write(TargetPath, Content);
            }
            return DefaultPackageInstallResult.Ok("файл установлен");
        }
    }
}