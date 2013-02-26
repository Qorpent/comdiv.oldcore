using System.IO;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Rules;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Filters{
    /// <summary>
    /// Фильтр перенаправления отрисовки (вынос логики выбора вьюхи из тела контроллера)
    /// в работе операется на ControllerExpert с именем "redirectview", возвращающий значения:
    /// 1. null - не менять вид
    /// 2. "ioc.get" - отменить вывод
    /// 3. d:NEWDIR - изменить viewfolder например d:/x/y/z == /a/b/c/index -> /x/y/z/index
    /// 4. f:FILE - изменить только имя вида без f:index2 == /a/b/c/index -> /a/b/c/index2
    /// 5. p:PREFIX - префиксировать имя p:ioc.get == /a/b/c/index -> /a/b/c/ioc.getindex
    /// 6. s:SUFFIX - добавить суффикс ioc.getforexcel == /a/b/c/report -> /a/b/c/ioc.getforexcel
    /// 7. x:VIEW - полная смера значения x:/a/index == /a/b/c/index -> /a/index
    /// </summary>
    public class RedirectViewFilter : Filter{
        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            if (context.Response.WasRedirected){
                return;
            }

            var c = controller as Controller;
            var p = "";
            if ((p = c.Params["returl"]).hasContent()){
                context.Response.RedirectToUrl(p);
                return;
            }
            if ((p = c.Params["renderCustomView"]).hasContent()){
                ((ControllerContext) controllerContext).SelectedViewName = p;
            }
            var newname = (string) ControllerExpert.Run("redirectview", null,
                                                        MvcContext.Create(context, controllerContext, controller));

            if (newname.yes()){
                if (newname == "ioc.get"){
                    ((Controller) controller).CancelView();
                }
                else{
                    var prefix = newname.Split(':')[0];
                    var value = newname.Split(':')[1];

                    if ("d" == prefix){
                        var oldviewname = Path.GetFileName(controllerContext.SelectedViewName);
                        ((ControllerContext) controllerContext).ViewFolder = value;
                        ((ControllerContext) controllerContext).SelectedViewName = Path.Combine(value, oldviewname);
                    }
                    else if ("f" == prefix){
                        ((ControllerContext) controllerContext).ViewFolder = Path.GetDirectoryName(newname);
                        ((ControllerContext) controllerContext).SelectedViewName = newname;
                    }
                    else if ("p" == prefix){
                        var dir = ((ControllerContext) controllerContext).ViewFolder;
                        var file = ((ControllerContext) controllerContext).SelectedViewName;
                        ((ControllerContext) controllerContext).SelectedViewName =
                            Path.Combine(dir, value + Path.GetFileName(file));
                    }
                    else if ("s" == prefix){
                        ((ControllerContext) controllerContext).SelectedViewName += value;
                    }
                    else if ("x" == prefix){
                        ((ControllerContext) controllerContext).SelectedViewName = value;
                    }
                }
            }
        }
    }
}