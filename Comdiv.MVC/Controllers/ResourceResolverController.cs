using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
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

namespace Comdiv.MVC.Controllers{
    [ControllerDetails("res")]
    [Public]
    public class ResourceResolverController : BaseController{
        private static readonly IDictionary<string, string> cache = new Dictionary<string, string>();

        [ActionDescription(ActionRole.Public, ActionSeverity.NonCritical, "Очищает кэш перенаправлений")]
        public void Reload(){
            cache.Clear();
            ResourceHelper.Reload();
        }

        [ActionDescription(ActionRole.Public, ActionSeverity.NonCritical, "Разрешает пути к ресурсам")]
        public void Get(string path){
            CancelView();
            CancelLayout();
            string correctPath = null;
            var key = myapp.usrName + path;
            if (!cache.ContainsKey(key)){
                cache[key] = GetCorrectPath(path);
            }
            correctPath = cache[key];
            if (null == correctPath){
                RenderText("the source file for {0} not found", path);
            }
            else{
                Response.RedirectToUrl(correctPath, true);
            }
        }

        private string GetCorrectPath(string path){
            string correctPath;
            correctPath = PathResolver.Resolve(path);
            //if (null != correctPath){
            //    correctPath = "~/" + PathResolver.ResolvePath(correctPath).Path;
            //}
            return correctPath;
        }
    }
}