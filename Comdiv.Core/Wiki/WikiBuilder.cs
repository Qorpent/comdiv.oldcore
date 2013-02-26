using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using System.Xml.XPath;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.MAS;
using Qorpent.Bxl;


namespace Comdiv.Wiki
{
	public class WikiBuilder
	{
		public WikiBuilderTask[] Build(string  taskcode, IConsoleLogHost logger) {
			var files = myapp.files.ResolveAll("wiki", "*.buildtask", true, null);
			logger.loginfo(files.Count()+" files for builder found");
			IList<WikiBuilderTask> tasks = new List<WikiBuilderTask>();
			foreach (var file in files) {
				logger.loginfo("file " +file +" start to load");
				var e =new BxlParser().Parse(File.ReadAllText(file),file);
				foreach (var te in e.XPathSelectElements("//task")) {
					var task = WikiBuilderTask.ReadFromXml(te); 
					tasks.Add(task);
					logger.loginfo("task with code "+task.Code+" added");
				}
				
			}
			var filter = taskcode.split();
			WikiBuilderTask[] result = null;
			if (0 == filter.Count) {
				result = tasks.ToArray();
			}else {
				result = tasks.Where(x => filter.Contains(x.Id)).ToArray();
			}
			var respository = myapp.ioc.get<IWikiRepository>() ?? new WikiRepository();
			foreach (var task in result) {
				logger.loginfo("task " +task +" started");
				var executor = task.CustomExecutor ?? getDefaultExecutor(task);
				executor.Execute(respository, task, logger);
				logger.loginfo("task " + task + " finished");
			}
			return result;
		}

		private IWikiBuilderTaskExecutor getDefaultExecutor(WikiBuilderTask task) {
			return myapp.ioc.get<IWikiBuilderTaskExecutor>(task.Type.ToString() + ".wiki.builder");
		}
	}
}
