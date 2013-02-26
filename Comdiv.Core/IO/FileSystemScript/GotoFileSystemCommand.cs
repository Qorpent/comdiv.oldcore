using System;
using Comdiv.Extensions;

namespace Comdiv.IO.FileSystemScript {
    [FileSystemCommand("goto")]
    public class GotoFileSystemCommand : FileSystemCommand
    {
        public GotoFileSystemCommand() {
            this.Code = "goto";
        }
        public string Id
        {
            get { return Args.get("id", ""); }
            set { Args["id"] = value; }
        }
        protected override void internalExecute() {
            int idx = 0;
            foreach (var fs in this.Programm.Commands) {
                if(fs is LabelFileSystemCommand) {
                    if(((LabelFileSystemCommand)fs).Id==this.Id) {
                        break;
                    }
                }
                idx += 1;
            }
            if(idx==Programm.Commands.Count) {
                Log.logerror("cannot find label "+Id);
                throw new Exception("cannot find label "+Id);
            }
            Programm.CurrentCommandIndex = idx;
        }
    }
}