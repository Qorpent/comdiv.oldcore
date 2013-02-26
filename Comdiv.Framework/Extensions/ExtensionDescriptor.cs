using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Xml.Smart;
using Qorpent.Bxl;

namespace Comdiv.Extensions
{
    public class ExtensionDescriptor
    {
        public string Name { get; set; }
        public string SourcePath { 
            get { return myapp.files.Resolve("~/extensionslib/" + Name,false); }
        }
        public bool IsInstalled {
            get {return TargetPath.hasContent(); }
        }
        IDictionary<string,string > manifest = null;
        public IDictionary<string ,string > Manifest {
            get {
                if(null==manifest) {
                    manifest  = new Dictionary<string, string>();
                    var file = Path.Combine(SourcePath, "manifest.bxl");
                    if(File.Exists(file)) {
                        var xml = new BxlParser().Parse(File.ReadAllText(file));
                        new SmartXml(xml).BindTo(manifest);
                    }
                }
                return manifest;
            }
        }
        public string TargetPath {get { return myapp.files.Resolve("extensions/" + Name, true); }}
        public void Install(string root = "") {
            root = "~/"+(root ?? "")+"/" + "extensions/"+Name;
            var folder = myapp.files.Resolve(root.Replace("//","/"), false);
            Directory.CreateDirectory(folder);

            var sync = new DirectorySynchronizer();
            sync.Source = SourcePath;
            sync.Target = folder;
            sync.Behaviour.DeleteFiles = true;
            sync.Prepare().Execute();

            //back copy
            sync = new DirectorySynchronizer();
            sync.Source = folder;
            sync.Target = SourcePath;
            sync.Prepare().Execute();

            var manifestinstalllist = this.Manifest.get("installers");
            if (manifestinstalllist.hasContent()) {
                var installersnames = manifestinstalllist.split(false, true, ';');
                foreach (var installersname in installersnames) {
                    var installer = (IExtensionInstaller)ioc.get(installersname);
                    installer.AfterInstall(this.Name);
                }
            }


            var installers = myapp.ioc.all<IExtensionInstaller>().Where(x => x.IsMatch(Name)).ToArray();
            foreach (var installer in installers) {
                installer.AfterInstall(Name);
            }
            
            myapp.files.Reload();
        }
        public void UnInstall() {

            var installers = myapp.ioc.all<IExtensionInstaller>().Where(x => x.IsMatch(Name)).ToArray();
            foreach (var installer in installers)
            {
                installer.BeforeDeinstall(Name);
            }

            var manifestinstalllist = this.Manifest.get("installers");
            if (manifestinstalllist.hasContent())
            {
                var installersnames = manifestinstalllist.split(false,true,';');
                foreach (var installersname in installersnames)
                {
                    var installer = (IExtensionInstaller)ioc.get(installersname);
                    installer.BeforeDeinstall(this.Name);
                }
            }


            

            Directory.Delete(TargetPath,true);
            myapp.files.Reload();
        }

    }
}
