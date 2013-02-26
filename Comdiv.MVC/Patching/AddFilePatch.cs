using System.Collections.Generic;
using Comdiv.Distribution;
using Comdiv.Extensions;
using Comdiv.Patching;


namespace Comdiv.MVC.Patching{
    public class AddFilePatch:MvcPatch{
        public AddFilePatch(){
            Root = "usr/";
        }
        public string Root { get; set; }
        public string Files { get; set; }
        public override IList<IPackageInstallTask> Tasks
        {
            get
            {
                if (base.Tasks.Count == 0)
                {
                    foreach (var file in Files.split() ){
                        base.Tasks.Add(new FileTask{TargetPath = Root+file,Content = Manager.FileSystem.Read(file)});
                    }
                }
                return base.Tasks;
            }
        }
    }
}