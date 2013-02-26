using System;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;

namespace Comdiv.IO.FileSystemScript {

    [FileSystemCommand("setup")]
    public class SetupFileSystemCommand : FileSystemCommand
    {
        protected override void internalExecute() {
            if (null != Programm) {
                foreach (var arg in this.Args) {
                    var val = arg.Value;
                    bool soft = false;
                    if(val.StartsWith("#")) {
                        val = val.Substring(1);
                        soft = true;
                    }
                    if (!soft || Programm.Parameters.get(arg.Key, "") == "") {
                        Programm.Parameters[arg.Key] = val;
                    }
                }
            }
        }
    }

    [FileSystemCommand("copy")]
    public class CopyFileSystemCommand : FileSystemCommand {
        
        public CopyFileSystemCommand():base() {
            this.Overwrite = false;
            this.Exclude = @"\.bak/";
            this.Recursive = true;
            this.KeepPaths = false;
            this.Code = "copy";
        }

        public string SrcDir {
            get { return setuparg(Args.get("srcdir")); }
            set { Args["srcdir"]=value; }
        }

        public string TrgDir
        {
            get { return setuparg(Args.get("trgdir")); }
            set { Args["trgdir"] = value; }
        }

        public bool Overwrite {
            get { return setuparg(Args.get("overwrite")).toBool(); }
            set { Args["overwrite"] = value.ToString(); }
        }

        public string Mask {
            get { return setuparg(Args.get("mask")); }
            set { Args["mask"] = value; }
        }

        public bool Recursive
        {
            get { return setuparg(Args.get("recursive")).toBool(); }
            set { Args["recursive"] = value.ToString(); }
        }

        public bool KeepPaths
        {
            get { return setuparg(Args.get("keeppaths")).toBool(); }
            set { Args["keeppaths"] = value.ToString(); }
        }

        public string Exclude {
            get { return setuparg(Args.get("exclude")); }
            set { Args["exclude"] = value; }
        }


        protected override void internalExecute() {
            var excludes = Exclude.split(false, true, ';');
            var sources = Filesystem.ResolveAll(SrcDir, Mask, true, Recursive);
            var srcdir = Filesystem.Resolve(SrcDir, true).normalizePath();
            foreach (var source in sources) {
                var src = source.normalizePath();
                Log.logdebug("src->"+src);
                if(null!=excludes.FirstOrDefault(x=>src.like(x))) continue;
                Log.logdebug("src not excluded");
                var filename = Path.GetFileName(src);
                var localpath = src.Replace(srcdir, "").normalizePath();
                var outfile = (TrgDir + "/" + (KeepPaths ? localpath : filename)).normalizePath();
                Log.logdebug("outfile->"+outfile);
                var trg = Filesystem.Resolve(outfile, false);
                Log.logdebug("trg->"+trg);
                var srcdate = new DateTime(1901, 1, 1);
                var trgdate = new DateTime(1900, 1, 1);
                if(!Overwrite) {
                    srcdate = File.GetLastWriteTime(src);
                    if(File.Exists(trg)) {
                        trgdate = File.GetLastWriteTime(trg);
                    }
                }
                if(srcdate>trgdate) {
                    Log.logdebug("need copy");
                    Directory.CreateDirectory(Path.GetDirectoryName(trg));
                    File.Copy(src,trg,true);
                    Log.loginfo("copyfile " + source+"->"+outfile);
                }
            }
        }
    }
}