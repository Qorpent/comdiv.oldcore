using System.Collections.Generic;
using System.Security.Principal;

namespace Comdiv.Application {
    public interface IWidgetRepository {
        WidgetRepository GetInstance();
        WidgetCollection GetMyWidgets();
        IEnumerable<Widget> GetMyWidgets(IPrincipal usr, string url);
        Widget[] GetAllWidgets();
    }
}