using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
using Comdiv.Web;

namespace Comdiv.MVC.Controllers{
    public class MvcPrepareHelper{
        public static void GenerateView(string resourceName, Type targetType, string viewFolderRoot){
            var dir = Directory.CreateDirectory(("~/views/" + viewFolderRoot).MapPath());
            var file = Path.Combine(dir.FullName,
                                    Regex.Match(resourceName, @"[^\.]+\.[^\.]+$", RegexOptions.Compiled).Value);

            var a = targetType.Assembly;

            if (!file.toFileInfo().Exists || file.toFileInfo().LastWriteTime < a.Location.toFileInfo().LastWriteTime){
                using (
                    var sr =
                        new StreamReader(
                            a.GetManifestResourceStream(a.GetName().Name + resourceName))){
                    using (var sw = new StreamWriter(file)){
                        sw.Write(sr.ReadToEnd());
                        sw.Flush();
                    }
                }
            }
        }
    }
}