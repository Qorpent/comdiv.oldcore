using System.Collections.Generic;
using System.IO;
using Comdiv.Application;
using Comdiv.MAS;
using Comdiv.Wiki;

namespace Castle.MonoRail.Views.Brail {
	public class BrailWikiBuilder : IWikiBuilderTaskExecutor {
		private BooViewEngine brail;
		private WikiRender render;

		public BrailWikiBuilder() {
			
		}
		public void Execute(IWikiRepository repository, WikiBuilderTask task, IConsoleLogHost logger) {
			this.brail = new BooViewEngine();
			brail.Options = new MonoRailViewEngineOptions();
			brail.Options.BaseType = "Castle.MonoRail.Views.Brail.WikiBrailBase";
			this.render = new WikiRender();
			var srcpages = new List<WikiPage>();
			foreach (var src in task.Sources)
			{
				foreach (var p in repository.Search(src))
				{
					srcpages.Add(p);
				}

			}
			foreach (var target in task.Targets) {
				var outfile = myapp.files.Resolve(target,false);
				
				var parameters = new Dictionary<string, object>()
				                 	{
				                 		{"task", task},
				                 		{"logger", logger},
				                 		{"repository", repository},
				                 		{"render", render},
				                 		{"pages", srcpages.ToArray()},
				                 	};
				var sw = new StringWriter();
				 brail.Process(task.View,task.Layout,sw, parameters);
				File.WriteAllText(outfile,sw.ToString());

			}
		}
	}
}