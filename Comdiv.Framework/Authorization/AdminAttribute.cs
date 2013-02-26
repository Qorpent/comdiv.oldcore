using System;
using Comdiv.Security;

namespace Comdiv.Authorization {
   // [Obsolete("Ушли от уровней, оставил только ролевую авторизацию")]
    public class AdminAttribute : RoleAttribute{
        public AdminAttribute() : base("ADMIN") {}
    }
}