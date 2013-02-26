using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Security;

namespace Comdiv.Controllers
{
    public class FirstRunController:BaseController
    {
        /// <summary>
        /// Goal of first run is to Deploy ExtensionsInstaller, FirstRun is only controller that is provided by default
        /// </summary>
        public void go()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Инициация запущена");
            if(!myapp.roles.IsAdmin()) {
                sb.AppendLine("Вы не являетесь администратором, проверяем первый запуск...");
                //не админам можно выполнять только в одном случае - нет записи суперюзера в security.map.bxl
                ensureSuperUserRun(sb);
            }
            
            sb.AppendLine("Инициация запущена");
            sb.AppendLine("Можете переходить к включению пакетов");
            sb.AppendLine(string.Format("<a href='{0}/extinstaller/index.rails'>ссылка</a>",
                                        Context.ApplicationPath));
        
            
            RenderText(sb.ToString().replace(@"[\r\n]+","<br/>"));

        }

        

        private void ensureSuperUserRun(StringBuilder sb) {
            var reader = new BxlApplicationXmlReader();
            var rolefiles = reader.Read("security.map.bxl");
            var su = rolefiles.Element("superuser");
            if(su!=null) {
                throw new SecurityException("Неавторизованный доступ к комманде первого запуска");
            }

            sb.AppendLine(
                "Это первый запуск системы без установленных настроек ролей, используем ваши учетные данные как данные старшего администратора");
            var topfile = myapp.files.Resolve("~/usr/security.map.bxl", true);
            string append = string.Format(@"
superuser '{0}'
map '{0}', 'ADMIN'",
                                          myapp.usrName.Replace("\\", "/").ToLower());
            if(topfile.noContent()) {
                myapp.files.Write("~/usr/security.map.bxl",append);
            }else {
                File.AppendAllText(topfile,append);
            }
            ((RoleResolver)myapp.roles).Reload();
            myapp.files.Reload();
            sb.AppendLine(
                "Теперь вы администратор");
        }
    }
}
