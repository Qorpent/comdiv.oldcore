using System.Security.Principal;

namespace Comdiv.Test.Extensions{
    public class thePrincipal{
        public static IPrincipal get(){
            return get(new string[] {});
        }

        public static IPrincipal get(string[] roles){
            return get("test", roles);
        }

        public static IPrincipal get(string name,params string[] roles){
            return new GenericPrincipal(new GenericIdentity(name), roles);
        }
    }
}