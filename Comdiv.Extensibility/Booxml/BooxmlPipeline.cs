using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Parser;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;

using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Booxml{
    public class BooxmlPipeline : Parse{
        public BooxmlPipeline(){

            
#if !LIB2
            this.InsertBefore(typeof(BooParsingStep), new BooxmlPreprocessor());

            this.InsertAfter(typeof(BooParsingStep),new BooxmlXmlCollector());         
#else
            this.InsertBefore(typeof(Boo.Lang.Compiler.Steps.Parsing), new BooxmlPreprocessor());
            this.InsertAfter(typeof(Parsing), new BooxmlXmlCollector());         
#endif
            this.InsertAfter(typeof (BooxmlXmlCollector), new ApplyGlobalsAndSubstitutionsStep());
          //  this.InsertAfter(typeof(ApplyGlobalsAndSubstitutionsStep), new PrintXmlStep());
#if !LIB2
            this.InsertAfter(typeof(BooParsingStep), new MacroAsElementStep());
#else
            this.InsertAfter(typeof(Parsing), new MacroAsElementStep());
#endif
            this.InsertAfter(typeof(MacroAsElementStep), new DefaultAttributesStep());
            this.InsertAfter(typeof(DefaultAttributesStep), new AssignsAreAttributes());
            this.InsertAfter(typeof(DefaultAttributesStep), new AssignsLiteralsAsTextStep());
            
            
        }
    }
}