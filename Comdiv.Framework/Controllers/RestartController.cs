using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Authorization;

namespace Comdiv.Controllers {
    [Admin]
    public class RestartController : BaseController {
        public void reload(int level) {
            var e = myapp.reload(level);
            if (e == null) {
                RenderText("reloaded");
            }else {
                Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
                RenderText(e.ToString());
            }
        }
        public void restart(int seconds) {
            AppRestartTimer.Start(seconds * 1000);
            RenderText("restarted");
        }
       
    }
}