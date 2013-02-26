using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdiv.Extensions
{
    public interface IExtensionInstaller {
        bool IsMatch(string extensioncode);
        void AfterInstall(string extensioncode);
        void BeforeDeinstall(string extensioncode);
    }
}
