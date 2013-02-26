using System.IO;
using Castle.MonoRail.Framework;
using Comdiv.Extensibility.Brail;
using Comdiv.Wiki;

namespace Castle.MonoRail.Views.Brail {
	public abstract class WikiBrailBase : BrailBase {
		public IWikiRepository repository;
		public IWikiRenderService wikirender;
		public WikiBuilderTask task;
		public WikiPage[] pages;
		public WikiRenderExecutor workrender;


		protected WikiBrailBase(BooViewEngine viewEngine, TextWriter output, IEngineContext context, IController __controller, IControllerContext __controllerContext) : base(viewEngine, output, context, __controller, __controllerContext) {}

		public override void InitProperties(Framework.IEngineContext myContext, Framework.IController myController, Framework.IControllerContext controllerContext)
		{
			base.InitProperties(myContext, myController, controllerContext);
			this.repository = controllerContext.PropertyBag["repository"] as IWikiRepository;
			this.wikirender = controllerContext.PropertyBag["render"] as IWikiRenderService;
			this.task = controllerContext.PropertyBag["task"] as WikiBuilderTask;
			this.pages = controllerContext.PropertyBag["pages"] as WikiPage[];
			this.workrender = this.wikirender.Start();
		}

		

		public void write(WikiPage page) {
			this.OutputStream.Write(this.workrender.Render(page));
		}
		
	}
}