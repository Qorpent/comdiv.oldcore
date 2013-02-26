using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Security;

namespace Comdiv.Authorization
{
    /// <summary>
    /// Основной фильтр авторизации доступа пользователя к действию
    /// определяет доступ на основе основного резольвера ролей уровня
    /// приложения.
    /// ADMIN - всегда можно
    /// ALLOW_[CONTROLLER]_[ACTION] - можно
    /// DENY_[CONTROLLER]_[ACTION] - нельзя
    /// ALLOW_[CONTROLLER] - можно
    /// DENY_[CONTROLLER] - нельзя
    /// Если есть у действия атрибут RolesAttribute - определяем по нему
    /// Если есть у контроллера атрибут RolesAttribute - определяем по нему
    /// По умолчанию - можно
    /// </summary>
    /// <remarks>выбрасывает исключение авторизации а не false</remarks>
    public class AuthorizeFilter:Filter 
    {

        /// <summary>
        /// Called when [before action].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override bool OnBeforeAction(IEngineContext context, IController controller, IControllerContext controllerContext) {
       
            var result =  AuthorizeContext(controllerContext, controller);
            //если не удалось авторизовать - исключение
            if(!result) {
                error(controller,controllerContext,myapp.usr);
                return false;
            }
            //иначе все ОК
            return true;
        }

        public bool AuthorizeContext(IControllerContext controllerContext, IController controller) {
            //ОБЩАЯ ПОЛИТИКА - ОТКРЫТОСТЬ ДЕЙСТВИЯ

            // сперва проверим не админ ли сам пользователь
            var usr = myapp.usr;
            if(myapp.roles.IsAdmin(usr)) return true; //админам можно
            
            // потом проверим не назначены ли пользователю спец-права на данное действие или контроллер
            var denyspecialrole = string.Format("DENY_{0}_{1}", controllerContext.Name, controllerContext.Action).ToUpper();
            if (myapp.roles.IsInRole(usr, denyspecialrole)) return false;
            var allowspecialrole = string.Format("ALLOW_{0}_{1}", controllerContext.Name, controllerContext.Action).ToUpper();
            if (myapp.roles.IsInRole(usr, allowspecialrole)) return true;
            var denycontrollerrolename = string.Format("DENY_{0}", controllerContext.Name).ToUpper();
            if (myapp.roles.IsInRole(usr, denycontrollerrolename)) return false;
            var allowcontrollerrolename = string.Format("ALLOW_{0}", controllerContext.Name).ToUpper();
            if (myapp.roles.IsInRole(usr, allowcontrollerrolename)) return true;

            //потом проверяем по атрибутам ролей
            IEnumerable<string> roles = getroles(controller, controllerContext);
            if (roles.Count() != 0) {
                if (roles.All(x => !myapp.roles.IsInRole(usr, x))) return false;
            }

            //если доехали до сюда, значит ошибок авторизации не было, значит дозволим выполнение
            return true;
        }

        /// <summary>
        /// Errors the specified controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="usr">The usr.</param>
        /// <remarks></remarks>
        private void error(IController controller, IControllerContext controllerContext, IPrincipal usr) {
            ((Controller)controller).Redirect("notauthorized","show",new Dictionary<string,string>
            {
                 {"name" , controllerContext.Name},
                 {"action" , controllerContext.Action},
                 {"user", usr.Identity.Name},
            });
           
        }

        /// <summary>
        /// Getroleses the specified controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private IEnumerable<string> getroles(IController controller, IControllerContext controllerContext) {
            RoleAttribute rolesattribute = null;
            //сначала ищем роли у действия (более глубокий уровень)
            var actionmethod = getActionMethod(controller, controllerContext);
            if (null != actionmethod) {
                rolesattribute =
                    actionmethod.GetCustomAttributes(typeof (RoleAttribute), true).OfType<RoleAttribute>().
                        FirstOrDefault();
            }
            // если у действия нет атрибута, ищем у всего контроллера
            if (null == rolesattribute) {
                rolesattribute =
                    controller.GetType().GetCustomAttributes(typeof(RoleAttribute), true).OfType<RoleAttribute>().FirstOrDefault();
            }
            //если атрибут найдейн - вернуть список ролей из атрибута
            if(null != rolesattribute) {
                return rolesattribute.Role.split();
            }
            //иначе вернуть пустой список, нет сопоставления ролей
            return new string[]{};
        }

        /// <summary>
        /// Gets the action method.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private MethodInfo getActionMethod(IController controller, IControllerContext controllerContext) {
            //NOTE: метод представленный здесь - быстрый но не охватывает всех случаев, например асинхронных и проч, но охватывает основной и как мне кажется нормальный паттерн
            var action = controller.GetType().GetMethod(controllerContext.Action,
                                                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                                                        BindingFlags.Public | BindingFlags.IgnoreCase);
            return action; //null is not error - it means that controller or action is untipical
        }
    }
}
