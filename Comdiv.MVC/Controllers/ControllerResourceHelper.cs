using System.IO;
using Castle.MonoRail.Framework.Helpers;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Controllers {
    /// <summary>
    /// Облегчает разрешение путей
    /// </summary>
    public class ControllerResourceHelper:AbstractHelper {

        public string getFile(string name) {
            //сначала пытаемся резольвить для расширений
            var local =
               myapp.files.Resolve(string.Format("extensions/{0}/{1}", ControllerContext.Name.ToLower(), name));
            if(local.noContent()) {
                //если не сканало резольвим для обычных контроллеров через views
                local = myapp.files.Resolve(string.Format("views/{0}/{1}", ControllerContext.Name.ToLower(), name));
            }
            if(local.noContent() && name.EndsWith(".js")) {
                local = myapp.files.Resolve(string.Format("scripts/{0}", name));
            }
            if (local.noContent() && name.EndsWith(".css"))
            {
                local = myapp.files.Resolve(string.Format("styles/{0}", name));
            }
            if (local.noContent())
            {
                local = myapp.files.Resolve(string.Format("images/{0}", name));
            }
            if (local.noContent()) {
                return "";
            }
            return ("/"+Context.ApplicationPath+"/"+ myapp.files.ResolveAsLocal(local)).Replace("//","/");
            
        }

        /// <summary>
        /// Finds css for controller and convert it to local HTTP friendly path,
        /// especially usefull for extensions
        /// </summary>
        /// <param name="name">The name.  if ommited - 'default'used</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getCss(string name = "default") {
            name = name ?? "default";
            return getFile(name + ".css");
        }
        /// <summary>
        /// Finds js for controller and convert it to local HTTP friendly path
        /// especially usefull for extensions
        /// </summary>
        /// <param name="name">The name. if ommited - 'default' used</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getJs(string name = "default")
        {
            name = name ?? "default";
            return getFile(name + ".js");
        }

        /// <summary>
        /// Finds images (PNG by default ext) for controller and convert it to local HTTP friendly path
        /// especially usefull for extensions,
        /// </summary>
        /// <param name="name">The name. if ommited - 'default.png', if no extension '.png' provided used</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getImg(string name = "default.png")
        {
            name = name ?? "default.png";
            var ext = Path.GetExtension(name);
            if(ext.noContent()) {
                ext = ".png";
            }
            return getFile(name);
        }
    }
}