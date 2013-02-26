using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
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

namespace Comdiv.MVC{
    public class ResourceHelper : IControllerAware{
        public static IDictionary<string, string> cache = new Dictionary<string, string>();
        private static bool ReloadSubscribed;

        public ResourceHelper(){
            if (!ReloadSubscribed){
                Subscribe();
            }
        }

        public string this[string localPath]{
            get{
                var result = Get(myapp.usr, localPath);
                if (null == result){
                    return ((Controller) MyController).Context.ApplicationPath + "/res/get.rails?path=" + localPath;
                }
                return ((Controller) MyController).Context.ApplicationPath + "/" + result;
            }
        }

        public IController MyController { get; set; }
        public IControllerContext MyContext { get; set; }

        #region IControllerAware Members

        public void SetController(IController controller, IControllerContext context){
            MyController = controller;
            MyContext = context;
        }

        #endregion

        public string Img(string name){
            return Img(name,
                       "");
        }

        public string Img(string name, string title){
            if (!name.Contains(".")){
                name += ".png";
            }
            var path = this["content/image/"+name];
            return string.Format("<img src='{0}' alt='{1}' title='{1}' />", path, title);
        }


        public string Style(string name){
            return string.Format("<link href='{0}' rel='Stylesheet' type='text/css'/>", this["content/style/"+name + ".css"]);
        }

        public static void Reload(){
            cache.Clear();
        }

        private static void Subscribe(){
            myapp.OnReload += (s, a) => Reload();
            ReloadSubscribed = true;
        }

        public string Get(IPrincipal principal, string path){
            if(path.EndsWith(".png") || path.EndsWith(".gif")){
                if(!path.StartsWith("content")){
                    path = "content/image/" + path;
                }
            }
            var key = principal.Identity.Name + path;
            if (!cache.ContainsKey(key)){
                cache[key] = GetCorrectPath(path);
            }
            return cache[key];
        }

        public string GetCorrectPath(string path){
            var correctPath = myapp.files.Resolve(path, true);
            if(null==correctPath)return null;
            correctPath = correctPath.Replace(myapp.files.Resolve("~/",false).Replace("\\","/"),"");
            //if (null != correctPath){
            //    correctPath = ioc.get<IFilePathResolver>().ResolvePath(correctPath).Path;
            //}
            return correctPath;
        }
    }
}