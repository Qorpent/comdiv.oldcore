using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Xml;

namespace Comdiv.MVC
{

  
    public abstract class MyBrailBase:BrailBase
    {
		
        public MyBrailBase(BooViewEngine viewEngine, TextWriter output, IEngineContext context, IController __controller, IControllerContext __controllerContext) : base(viewEngine, output, context, __controller, __controllerContext) {}
    }
}
