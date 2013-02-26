using System;
using System.Collections.Generic;
using System.Text;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Wiki;
using Qorpent.Mvc;

namespace Comdiv.Framework.Quick {
	public class WikiRender : IRender {

		

		private void generateNewWiki(string wikiname, IMvcContext context, IWikiRepository repo)
		{
			var executor = context.Application.MvcFactory.GetAction(context);
			try {
				var sb = new StringBuilder();
				sb.AppendFormat("<h1>JSONAPI: {0}</h1>", context.ActionName);
				sb.AppendFormat("<ul><li>{0}</li></ul>", executor.GetType().FullName);

				sb.AppendLine("<p>Документация на данный момент отсутствует. Данный файл сформирован автоматически</p>");
				var page = new WikiPage
				           	{
				           		Level = FileLevel.sys,
				           		Code = wikiname,
				           		Content = sb.ToString(),
				           		Properties = new Dictionary<string, string>
				           		             	{
				           		             		{"type", "quickdoc"},
				           		             		{"quickname", context.ActionName},
				           		             	}
				           	};
				repo.Save(page);
			}finally {
				context.Application.MvcFactory.ReleaseAction(context);
			}
		}

		public void Render( IMvcContext context) {
			var wikiname = "_quicks/" + context.ActionName.Replace(".", "_");
			var repo = myapp.ioc.get<IWikiRepository>() ?? new WikiRepository();
			if (!repo.Exists(wikiname))
			{
				generateNewWiki(wikiname, context, repo);
			}
			var render = myapp.ioc.get<IWikiRenderService>();
			context.Output.Write(render.Start().Render(repo.Get(wikiname)));
		}

		public void RenderError(Exception error, IMvcContext context) {
			context.Output.Write(error.ToString());
		}

		public bool NeedResult {
			get { return false; }
		}
	}
}