using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Castle.MonoRail.Views.Brail;
using Qorpent.Mvc;


namespace Comdiv.Framework.Quick {
	public class BooViewEngineAdapterForQWeb:IViewEngine {
		private BooViewEngine engine;

		public BooViewEngineAdapterForQWeb() {
			this.engine = new BooViewEngine();
		}
		

		public void Process(string viewname, string masterviewname, object viewdata, IMvcContext context) {
			var dict = new Dictionary<string, object>();
			foreach (var parameter in context.Parameters)
			{
				dict[parameter.Key] = parameter.Value;
			}
			dict["_context"] = context;
			dict["_result"] = viewdata;
		    dict["siteroot"] = ((MvcContext)context).NativeAspContext.Request.ApplicationPath;
            if (null != viewdata) {
                if (viewdata.GetType().GetCustomAttributes(typeof (CompilerGeneratedAttribute), true).Length > 0) {
                    foreach (var p in viewdata.GetType().GetProperties()) {
                        dict[p.Name] = p.GetValue(viewdata, null);
                    }
                }
            }
		    engine.Process(viewname,masterviewname,context.Output,dict);
		}

		
	}
}