using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Controllers{
    public class WorkspacePartition : BasePartition{
        public WorkspacePartition(){
            ControllerName = "";
            ActionName = "";
            Role = WorkspaceZone.Bottom.ToString();
        }

        public WorkspacePartition(WorkspaceZone zone) : this(zone, null) {}
        public WorkspacePartition(WorkspaceZone zone, IBrailRender render) : this(zone, 0, render) {}
        public WorkspacePartition(WorkspaceZone zone, int idx) : this(zone, idx, null) {}
        public WorkspacePartition(WorkspaceZone zone, int idx, IBrailRender render) : this(zone, idx, render, "") {}

        public WorkspacePartition(WorkspaceZone zone, int idx, IBrailRender render, string roles)
            : this(zone, idx, render, roles, "") {}

        public WorkspacePartition(WorkspaceZone zone, int idx, IBrailRender render, string roles, string users)
            : this(zone, idx, render, roles, users, "") {}

        public WorkspacePartition(WorkspaceZone zone, int idx, IBrailRender render, string roles, string users,
                                  string viewName) : this(){
            Role = zone.ToString();
            Idx = idx;
            Roles = roles ?? "";
            Users = users ?? "";
            ViewName = viewName;
            RenderObject = render;
        }
    }
}