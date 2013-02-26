using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Cfg{
    /// <summary>
    /// Окружение веб-приложения
    /// </summary>
    /// <remarks>С целью уменьшения различных magic-string</remarks>
    public interface IWebEnvironment{
        string GetHomePage();
        string GetPrintVersionCheckboxPrefix();
        Char GetDelimiter();
        string GetUserDataDirectory();
    }
}