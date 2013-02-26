using System;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler.Steps;
using Comdiv.Brail;
using Comdiv.Extensibility.Brail;
using MvcContrib.Comdiv.Brail;
using MvcContrib.Comdiv.Extensibility.TestSupport;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    public class BrailMacroTestBase : BaseMacroTestFixture<ForeachMacro> {
        public void checkHtml(string code, string html){
            checkHtml(code, null, html);
        }
        protected override Boo.Lang.Compiler.CompilerPipeline preparePipeline(Boo.Lang.Compiler.CompilerPipeline pipeline)
        {
#if !LIB2
            pipeline.InsertAfter(typeof (Boo.Lang.Parser.BooParsingStep), new ExpandBmlStep());
#else
			pipeline.InsertAfter(typeof(Parsing), new ExpandBmlStep());
			pipeline.InsertAfter(typeof(ExpandBmlStep),
													 new InterpolationUnescapeStep());
#endif
            return pipeline;
        }

        public void checkHtml(string code,object viewdata, string html){
            html = simplifyHtml(html);
            var result = MyBrail._Process(code, viewdata);
            var sresult = simplifyHtml(result);
            if(sresult!=html){
                Console.WriteLine(result);
                Assert.AreEqual(html, result);
            }
            
        }

        private string simplifyHtml(string html) {
            html = html.Replace("\"", "'");
            html = Regex.Replace(html,@"(\s)\s+", "$1");
            return html.Trim();
        }
    }
}