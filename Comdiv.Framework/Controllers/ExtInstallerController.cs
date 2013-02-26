using System;
using System.Linq;
using Comdiv.Authorization;
using Comdiv.Extensions;

namespace Comdiv.Controllers {
    [Admin]
    public class ExtInstallerController:BaseController {
        public void Index() {
            PropertyBag["extensions"] = new ExtensionsEnumerator().GetAllExtensions().OrderBy(x => x.Name).ToArray();
        }
        public void Install(string name, string root) {
            if (name.hasContent()) {
                new ExtensionsEnumerator().GetAllExtensions().First(x => x.Name.ToLower() == name.ToLower()).Install(
                    root);
            }  else {
                UpdateAll();
            }
            RenderText("OK");
        }
        public void UnInstall(string name) {
            if (name.hasContent()) {
                new ExtensionsEnumerator().GetAllExtensions().First(x => x.Name.ToLower() == name.ToLower()).UnInstall();
            }else {
                UnInstallAll();
            }
            RenderText("OK");
        }

        public void UnInstallAll() {
            foreach (var ext in new ExtensionsEnumerator().GetAllExtensions().Where(x => x.IsInstalled))
            {
                ext.UnInstall();
            }
            RenderText("OK");
        }

        public void UpdateAll() {
            foreach (var ext in new ExtensionsEnumerator().GetAllExtensions().Where(x=>x.IsInstalled))
            {
                ext.Install();
            }
            RenderText("OK");
        }
    }
}