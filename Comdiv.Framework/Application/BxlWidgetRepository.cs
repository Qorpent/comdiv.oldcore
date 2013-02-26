using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.IO;

namespace Comdiv.Application
{
    public class BxlWidgetRepository:WidgetRepository
    {
        public BxlWidgetRepository() {
            this.XmlReader = new BxlApplicationXmlReader{TotalSearch=true, Except=@"[\\/]extensionslib[\\/]"};
        }
    }
}
