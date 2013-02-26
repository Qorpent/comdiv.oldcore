using System;
using System.Linq;
using System.Text;
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


    /// <summary>
    /// Контроллер обслуживает задачи уровня приложения
    /// </summary>
    /// 
    /// 
    [Public]
    public class PublicController : BaseController{
        [Public]
        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "вывод учетных данных текущего пользователя"
            )]
        public void Me(string testRole){
            var content = new StringBuilder();
            content.AppendLine(string.Format("Domain:{0}<br/>", myapp.usrDomain));
            content.AppendLine(string.Format("User Name:{0}<br/>", myapp.usrName));
            content.AppendLine(string.Format("Current Role:{0}<br/>", myapp.roles.GetRoles().FirstOrDefault()));
            if (testRole.hasContent()){
                content.AppendLine(string.Format("Testing Role ({0}):{1}<br/>", testRole, myapp.roles.IsInRole(testRole)));
            }
            RenderText(content.ToString());
        }

        [Public]
        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "установка значений профиля текущего пользователя"
            )]
        public void Set(string propName, string value, string type){
            if(propName.noContent()){
                throw new ArgumentNullException("propName");
            }
            if(type.noContent()){
                type = "str";
            }
            var _type = ReflectionExtensions.ResolveTypeByWellKnownName(type);
            if(null==_type){
                throw new ArgumentException("this type is not supported "+type);
            }
            var val = value.to(_type);
            myapp.getProfile().Set(propName, val);
            CancelView();
        }


        [Public]
        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "вывод параметра профиля текущего пользователя"
            )]
        public void Get(string propName)
        {
            var val = myapp.getProfile().Get<object>(propName,null);
            if(null==val){
                RenderText("NULL");
            }else{
                RenderText(val.ToString());    
            }
            
        }

        [Public]
        public void echo(){
            RenderText("OK");
        }
    }
}