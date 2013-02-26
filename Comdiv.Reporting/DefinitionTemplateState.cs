using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Reporting{
    [Flags]
    public enum DefinitionTemplateState{
        None = 0,
        Template = 1,
        Filled = 2,
        Applyed = 4,
    }
}