using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rules.Core.Rules{
    public class RuleDescriptionAttribute : Attribute{
        public string RuleInfo { get; set; }
        public string TestInfo { get; set; }
        public string ActInfo { get; set; }
    }
}