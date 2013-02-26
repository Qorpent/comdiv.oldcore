using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Extensions;

namespace Comdiv.Controllers {
    public class OutputSelectionFilter : Filter
    {
        protected override bool OnBeforeAction(IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            var c = controller as BaseController;
            if (c.LayoutName.noContent() && c.Params.AllKeys.Contains("_layout"))
            {
                c.LayoutName = c.Params["_layout"];
            }
            if(c.Params.AllKeys.Contains("_view")) {
                c.SelectedViewName = c.Params["_view"];
            }
            if(c.Params.AllKeys.Contains("_mime")) {
                c.Response.ContentType = c.Params["_mime"];
            }
            return true;
        }

        
    }
}