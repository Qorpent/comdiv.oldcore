using System;

namespace Comdiv.Security {
    public class RoleAttribute:Attribute{
        public RoleAttribute(string role){
            this.Role = role;
        }

        public string Role { get; private set; }
    }
}