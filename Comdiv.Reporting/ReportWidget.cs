using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Reporting{
    public class ReportWidget:IViewCall{
        public string Code { get; set; }
        public string Name { get; set; }
        public bool OnPrepare { get; set; }
        public bool OnRender { get; set; }
        public string View { get; set; }
        private readonly IDictionary<string, object> _parameters = new Dictionary<string, object>();
        public IDictionary<string, object> Parameters{
            get { return _parameters; }
            
        }

        public string Zone { get; set; }
        public int Idx { get; set; }
        public string Text { get; set; }

        public string Role { get; set; }
        public string Condition { get; set; }

        public string SrcXml { get; set; }
    }
}