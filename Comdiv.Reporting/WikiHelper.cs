using System.Collections.Generic;
using Comdiv.Application;
using Comdiv.Extensibility.Brail;
using Comdiv.Inversion;
using Comdiv.Wiki;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Reporting
{
    public class WikiHelper : IWikiHelper, IReportDefinitionExtension //, IAttachToView
    {
        private IReportDefinition report;

        public void Contextualize(IReportDefinition definition)
        {
            this.report = definition;
            this.repos = this.repos ?? InversionExtensions.get<IWikiRepository>((IInversionContainer) myapp.ioc) ?? new WikiRepository();
            this.render_ = this.render_ ?? InversionExtensions.get<IWikiRenderService>((IInversionContainer) myapp.ioc) ?? new WikiRender();
            this.wrender = this.wrender ?? this.render_.Start();
        }
        
        Stack<BrailBaseCommon> viewstack = new Stack<BrailBaseCommon>();
        private BrailBaseCommon view;
        private IWikiRepository repos;
        private IWikiRenderService render_;
        private WikiRenderExecutor wrender;

        public void Push(BrailBaseCommon view)
        {
            if(viewstack.Count==0 || viewstack.Peek() != view)
            {
                viewstack.Push(view);
                this.view = viewstack.Peek();
            }
        }

        public void Pop(BrailBaseCommon view)
        {
            if(viewstack.Count!=0 && viewstack.Peek() == view)
            {
                viewstack.Pop();
                this.view = viewstack.Peek();
            }
        }

        public WikiPage get(string code)
        {
            return repos.Get(code);
        }
        public WikiPage[] find(string pattern)
        {
            return repos.Search(pattern);
        }
        public string render(WikiPage page)
        {
            return this.wrender.Render(page);
        }

        public string render(string code)
        {
            return render(get(code));
        }

        public string saferender(string code) {
            var page = get(code);
            if (null == page) return "";
            return render(page);
        }
    }
}