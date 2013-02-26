using System;

namespace Comdiv.Extensibility.Brail
{
    public class BrailSourceResolver: ViewSourceResolverBase
    {
        protected override string getDefaultExtension() {
            return ".brail";
        }

        protected override string[] getProbePaths() {
            return new[]
                       {
                           "~/usr/views/",
                           "~/usr/extensions/",
                           "~/mod/views/",
                           "~/mod/extensions/",
                           "~/sys/views/",
                           "~/sys/extensions/",
                           "~/views/",
                           "~/extensions/"
                       };
        }
    }
}
