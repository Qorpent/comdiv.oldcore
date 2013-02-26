using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Castle.MonoRail.Framework;
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
    public class AclList{
        public AclList(){
            Items = new List<Entity>();
        }

        public string Name { get; set; }
        public IList<Entity> Items { get; protected set; }
    }

    public interface IAclSource{
        AclList Get(IPrincipal principal);
    }


    [Admin]
    [Layout("workspace")]
    public class AclController : BaseController{
        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Показывает все активные права ACL"
            )]
        public void all(){
            PropertyBag["rules"] = acl.provider.Rules.OrderBy(x => x, new AclRuleComparer());
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Стартовая страница просмотра ACL"
            )]
        public void index(){
            var manager = Container.get<IAclProfileManager>();
            PropertyBag["items"] = manager.Enumerate().ToList();
            PropertyBag["current"] = manager.GetCurrent();
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Сохранение нового профиля"
            )]
        public void saveas(string code, string comment){
            var manager = Container.get<IAclProfileManager>();
            manager.SaveCurrentAs(code, comment);
            RedirectToAction("index");
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "активация профиля"
            )]
        public void activate(string code){
            var manager = Container.get<IAclProfileManager>();
            manager.Activate(code);
            myapp.reload();
            RedirectToAction("index");
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Загрузка профиля на правку"
            )]
        public void edit(string code){
            CancelLayout();
            var manager = Container.get<IAclProfileManager>();
            var existed = manager.Get(code);
            PropertyBag["ro"] = false;
            PropertyBag["item"] = existed;
            existed.Tag = myapp.files.Read(existed.Name);
            if (!existed.Name.StartsWith("usr/")){
                PropertyBag["ro"] = true;
            }
            PropertyBag["code"] = code;
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Обновление профиля после правки"
            )]
        public void update(string code, string txt){
            var manager = Container.get<IAclProfileManager>();
            var existed = manager.Get(code);
            myapp.files.Write(existed.Name, txt);
            RedirectToAction("index");
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Проверочный вызов системы ACL для проверки доступа"
            )]
        public void test() {}

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Сам метод проверки"
            )]
        public void exec(string usr, string roles, string token, string permission){
            CancelLayout();
            PropertyBag["result"] = "";
            var principal = usr.hasContent()
                                ? new GenericPrincipal(new GenericIdentity(usr), roles.split().ToArray())
                                : myapp.usr;
            if (token.hasContent()){
                PropertyBag["result"] = acl.get(token, permission, "", principal);
            }
            PropertyBag["items"] = Container.all<IAclSource>().Select(x => x.Get(principal)).ToList();
        }
    }
}