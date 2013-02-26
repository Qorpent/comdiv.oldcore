using Comdiv.Security;

namespace Comdiv.Authorization {
    public class PublicAttribute : RoleAttribute{
        public PublicAttribute() : base("DEFAULT") {
            
        }
    }
}