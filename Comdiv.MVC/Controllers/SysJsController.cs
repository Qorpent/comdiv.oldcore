using System.Collections.Generic;
using System.Web;
using Castle.MonoRail.Framework;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.MVC.Controllers{
    [Public]
    [Helper(typeof(ResourceHelper), "resx")]
    public class SysJsController : SmartDispatcherController{
        [Cache(HttpCacheability.Public, Duration = 600)]
        public void resourcelist(){
            IDictionary<string, string> map = new Dictionary<string, string>();
            var allcss = myapp.files.ResolveAllAsLocal("content/style", "*.css");
            foreach (var s in allcss){
                var css = s.replace(@"\w+/content/style/", "");
                if (!map.ContainsKey(css)){
                    map[css] = s;
                }
            }
            var alljs = myapp.files.ResolveAllAsLocal("~/scripts", "*.js",false);
            foreach (var s in alljs)
            {
                var js = s.replace(@"[\s\S]*?scripts/", "");
                if (!map.ContainsKey(js))
                {
                    map[js] = s;
                }
            }
            PropertyBag["map"] = map;
        }
    }
}