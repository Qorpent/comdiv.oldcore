using System.Linq;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Rules{
    public class ControllerRuleLoader : IControllerRuleLoader{
        public ControllerRuleLoader(){
            PathResolver = myapp.files;
        }

        public IFilePathResolver PathResolver { get; set; }

        #region IControllerRuleLoader Members

        public ControllerRuleGroup Load(string expertKey){
            var path = "controller.rules/" + expertKey + ".rules.xml"; 

            var expert = new ControllerRuleGroup();
            var xml = PathResolver.ReadXml(path, null,
                                           new ReadXmlOptions{ApplyXslt = true, Merge = true});
            expert.LoadFromXml(XElement.Parse(xml));
            return expert;
        }

        #endregion
    }
}