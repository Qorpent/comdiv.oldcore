using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Extensions;

namespace Comdiv.Wiki
{
	public class WikiBuilderTask
	{
		public WikiBuilderTask() {
			Parameters = new Dictionary<string, string>();
			Sources = new List<string>();
			Targets = new List<string>();
			Extensions = new List<object>();

		}
		public string Id { get; set;}
		public string Code { get; set; }
		public WikiBuilderTaskType Type { get; set; }
		public List<string > Sources { get; private set; }
		public List<string > Targets { get; private set; }
		public List<object > Extensions { get; private set; }
		public string Result { get; set; }
		public IWikiBuilderTaskExecutor CustomExecutor { get; set; }
		public IDictionary<string, string> Parameters { get; private set; }
		public string View { get; set; }

		public string Layout { get; set; }

		public static WikiBuilderTask ReadFromXml (XElement x) {
			var result = new WikiBuilderTask();
			x.applyTo(result);
			foreach (var e in x.Elements("source")) {
				result.Sources.Add(e.idorvalue());
			}
			foreach (var e in x.Elements("target"))
			{
				result.Targets.Add(e.idorvalue());
			}
			foreach (var e in x.Elements("extension"))
			{
				result.Extensions.Add(e.idorvalue().toType().create<object>());
			}
			foreach (var e in x.Elements("param")) {
				result.Parameters[e.attr("id")] = e.Value;
			}
			if(WikiBuilderTaskType.Custom==result.Type) {
				result.CustomExecutor = x.attr("executor").toType().create<IWikiBuilderTaskExecutor>();
			}
			return result;
		}
	}
}
