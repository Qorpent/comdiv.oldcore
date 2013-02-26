using Comdiv.MVC.Report;
using Comdiv.Wiki;

namespace Comdiv.MVC.Wiki {
    public class WikiReportParameterLinkRender : WikiPageLinkRender
    {

        public WikiReportParameterLinkRender()
        {
            this.Idx = 900;
            this.Folder = "rp";
        }
        protected override string getText(string code, string title, bool existed) {
            code = code.Replace("rp/", "");
            var p = new ReportParametersRepository().Get(code);
            return p.Code + " (" + p.Name + ")";
        }
        protected override string getTitle(string code, string title, bool existed)
        {
            code = code.Replace("rp/", "");
            return base.getTitle(code, getText(code,title,existed), existed);
        }
    }
}