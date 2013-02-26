using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Controllers;

namespace Comdiv.Security
{
    [Admin]
    public class RoleController:BaseController
    {
        public void assign(string username, string  role) {
            new RoleOperator().AssignToRole(username,role);
            RenderText("OK");
        }
        public void revoke(string username, string role)
        {
            new RoleOperator().RemoveFromRole(username, role);
            RenderText("OK");
        }
        
        public void test(string username,string  role) {
            RenderText(myapp.roles.IsInRole(username.toPrincipal(),role).ToString());
        }

        [Public]
        public void testme(string role) {
            RenderText(myapp.roles.IsInRole(myapp.usr, role).ToString());
        }
    }
}
