using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdiv.Extensibility
{
    [AttributeUsage(AttributeTargets.Assembly,AllowMultiple = true)]
    public class AssemblyBooMacroNamespaceAttribute:Attribute
    {
        public AssemblyBooMacroNamespaceAttribute(string ns) {
            this.Namespace = ns;
        }
        public string Namespace { get; private set; }
    }
}
