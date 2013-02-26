using System.Collections.Generic;
using System.IO;
using Comdiv.Application;

namespace Comdiv.Extensions {
    public class ExtensionsEnumerator {
        public IEnumerable<ExtensionDescriptor> GetAllExtensions() {
            var root = myapp.files.Resolve("~/extensionslib", true);
            if(root.hasContent()) {
                foreach (var extfolder in Directory.EnumerateDirectories(root)) {
                    yield return new ExtensionDescriptor{Name = Path.GetFileName(extfolder)};
                }
            }
        }
    }
}