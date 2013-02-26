using System.Web;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Framework
{
	public class QuickHttpHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context) {
			var path = context.Request.Url.AbsolutePath;
			var elements = path.Split('/');
			var name = elements[2] + '.' + elements[3];
			var executor = myapp.ioc.get<IHttpHandler>(name);
			executor.ProcessRequest(context);
		}

		public bool IsReusable {
			get { return true; }
		}
	}
}
