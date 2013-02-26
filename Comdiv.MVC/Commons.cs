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

namespace Comdiv{
    /// <summary>
    /// Основные константы
    /// </summary>
    public static class Commons{
        public static string BaseWebEnvironmentName = "webenv";
        public static char DefaultDelimiter = Convert.ToChar("_");
        public static string DefaultHomePage = "index.html";
        public static string DefaultPrintVersionCheckboxPrefix = "pchk";
        public static string DefaultUserData = @"user\data";
    }
}