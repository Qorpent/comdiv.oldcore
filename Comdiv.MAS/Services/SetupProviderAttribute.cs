using System;

namespace Comdiv.MAS {
    public class SetupProviderAttribute :Attribute {
        public SetupProviderAttribute(Type type) {
            this.Type = type;
        }
        public Type Type { get; set; }
    }
}