using System.Collections.Generic;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.MVC.Controllers {
    public static class WorkbenchControllerExtension
    {
        public static void PrepareWorkbench(Controller controller)
        {
            foreach (var prop in Enumerable.ToArray<KeyValuePair<string, object>>(myapp.getProfile().GetAsDictionary()))
            {
                if (prop.Key.StartsWith("wb."))
                {
                    var key = prop.Key.Substring(3);

                    controller.PropertyBag[key] = prop.Value;

                }

            }
        }
    }
}