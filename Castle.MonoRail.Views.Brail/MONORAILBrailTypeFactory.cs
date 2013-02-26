using System;
using System.IO;
using Comdiv.Extensibility.Brail;

namespace Castle.MonoRail.Views.Brail {
    public class MONORAILBrailTypeFactory:ViewTypeFactory {
        public MONORAILBrailTypeFactory(IViewSourceResolver resolver, ViewEngineOptions options):base(resolver,options,null) {
            this.Compiler = new BrailCompiler();
            ((BrailCompiler)this.Compiler).PreparePipeline += MONORAILBrailTypeFactory_PreparePipeline;
        }

        void MONORAILBrailTypeFactory_PreparePipeline(Boo.Lang.Compiler.CompilerPipeline obj)
        {
            obj.Replace(typeof (TransformToBrailStep), new MONORAILTransformToBrailStep(this.Options));
        }

        public BrailCompiler MyCompiler {
            get { return Compiler as BrailCompiler; }
        }
        
    }

}