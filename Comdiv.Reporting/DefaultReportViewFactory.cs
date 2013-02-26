using System;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Reporting{
    public class DefaultReportViewFactory:IReportViewFactory{
        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.Container;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public void Process(IReportDefinition definition){
            var dynamic = definition as IReportDefinitionDynamicView;
            if (null == dynamic) return;
            var code = definition.Code.Replace(".", "_");
            var dir = "~/usr/views/report/auto/" + code;
            string generatorname = dynamic.PrepareViewGenerator;
            if (generatorname.hasContent())
            {
                string name = "prepare";
                dynamic.PrepareView = "auto/"+ code + "/" + name;
                generate(dynamic, generatorname, name, dir);
            }
            generatorname = dynamic.RenderViewGenerator;
            if (generatorname.hasContent())
            {
                string name = "render";
                dynamic.RenderView = "auto/" + code + "/" + name;
                generate(dynamic, generatorname, name, dir);
            }
        }

        private void generate(IReportDefinitionDynamicView dynamic, string generatorname, string name, string dir) {
            IReportViewGenerator generator = null;
            var type = Type.GetType(generatorname);
            if (null != type)
            {
                generator = type.create<IReportViewGenerator>();
            }
            else{
                generator = Container.get<IReportViewGenerator>(generatorname);
            }
            string viewcontent = generator.Generate(dynamic,dir);
            string resultfilename = dir + "/" + name + ".brail";
            myapp.files.Write(resultfilename, viewcontent);
        }
    }
}