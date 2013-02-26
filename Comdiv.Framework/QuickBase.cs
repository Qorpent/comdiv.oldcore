using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Comdiv.Extensions;

namespace Comdiv.Framework
{
	public abstract class QuickBase:IHttpHandler {
		protected HttpContext context;
		protected IDictionary<string, object> _resolvecache;
		private object result;
		protected T get<T>(string name, T def = default(T)) {
			if(null==_resolvecache)_resolvecache = new Dictionary<string, object>();
			if (_resolvecache.ContainsKey(name)) return _resolvecache[name].to<T>();
			var result = def;
			if(-1!=Array.IndexOf(context.Request.Form.AllKeys,name)) {
				result = context.Request.Form[name].to<T>();
			}else {
				if(-1!=Array.IndexOf(context.Request.QueryString.AllKeys,name)) {
					result = context.Request.QueryString[name].to<T>();
				}
			}
			_resolvecache[name] = result;
			return result;
		}
		public void ProcessRequest(HttpContext context) {
			this.context = context;
			try {
				initialize();
				validate();
				authorize();
				this.result = process();
				render(result);
			}
			catch (Exception ex) {
				renderError(ex);
			}
		}

		protected virtual void initialize() {
			
		}

		protected  virtual void validate() {
			
		}

		protected abstract void renderError(Exception ex);

		protected virtual void authorize() {
			
		}

		protected virtual object process() {
			return null;
		}
		protected abstract void render(object obj);

		public bool IsReusable {
			get { return true; }
		}
	}

	public abstract class QuickJSON : QuickBase {
		protected override void render(object obj) {
			if (get("ajax", false)) {
				context.Response.ContentType = "application/json";
			}else {
				context.Response.ContentType = "text/javascript";
			}
			if (null == obj) context.Response.Write("null");
			else {
				context.Response.Write(obj.toJSON());
			}
		}
		protected override void renderError(Exception ex) {
			context.Response.StatusCode = 403;
			//context.Response.Status = "Error: " + ex.GetType().Name + " " + ex.Message;
			context.Response.Write(ex.ToString().toJSON());
		}
	}
}
