using Comdiv.Extensions;

namespace Comdiv.IO.FileSystemScript {
    [FileSystemCommand("label")]
    public class LabelFileSystemCommand : FileSystemCommand
    {
        public LabelFileSystemCommand() {
            this.Code = "label";
        }
        public string Id {
            get { return Args.get("id", ""); }
            set {  Args["id"] = value; }
        }

        protected override void internalExecute() {
            
        }
    }
}