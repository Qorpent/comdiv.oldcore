using System;
using System.Linq;
using System.Text;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;

using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.MAS;
using Comdiv.MVC.Wiki;
using Comdiv.Wiki;

namespace wikibuilder
{




	public class Program:MasConsoleApplication
	{
		public Program() {
			this.CanIgnoreMas = true;
			this.IgnoreMasByDefault = false;
		
		}

		protected override void initialize()
		{
			base.initialize();
			myapp.ioc.set<IWikiRepository, WikiRepository>();
			myapp.ioc.set<IWikiPersistenceProvider, WikiPersistenceProvider>();
			myapp.ioc.AddTransient("View.wiki.builder", typeof (BrailWikiBuilder));
			myapp.ioc.AddTransient("row.wikilinkrender", typeof (WikiRowLinkRender));
		}

		static void Main(string[] args)
		{
			new Program().Run(args);
		}

		protected override void execute() {
			var result = new WikiBuilder().Build(this.Args.get("filter"), this);
		}
	}
}
