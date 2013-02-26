using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;

namespace Comdiv.MAS
{
    public class DefaultMasProcessStarter : IMasProcessStarter {
        public MasProcessStartInfo Run(MasProcessStartInfo infobase) {
            var info = infobase.Copy();
            if(info.FileName.noContent()) {
                info.FileName = resolveExe(info.Name);
            }else {
                if(info.Name.noContent()) {
                    info.Name = resolveName(info.FileName);
                }
            }
            if (info.FileName.noContent())
            {
                throw new MasException("cannot find agent " + info.Name);
            }
            if(!File.Exists(info.FileName)) {
                throw new MasException("cannot find file " + info.FileName);
            }
            if(info.Code.noContent()) {
                info.Code = Guid.NewGuid().ToString();    
            }
            info.Args = (info.Args ?? "") + " --mas-process-code " + info.Code;
            var process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo(info.FileName, info.Args);
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(info.FileName);
            info.NativeProcess = process;
            process.Start();
            return info;
        }

        protected string resolveName(string fileName) {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        protected virtual string resolveExe(string name)
        {
            if (name.Contains("/"))
            {
                return myapp.files.Resolve(name, true);
            }
            return myapp.files.ResolveAll("~/", name + ".exe", true, true, null).FirstOrDefault();
        }
    }
}
