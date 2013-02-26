///============================================================================
/// TITLE: UsersActionLogger.cs
///
/// CONTENTS:
///
/// Служит для логгирования действий пользователя при работе с пользовательским интерфейсом.
/// Является централизованной точкой логгирования всех действий пользователя, что предоставляет возможность 
/// включать/выключать логгирование, задавать направление вывода сообщений - file/udp/база данных и т.д.
/// <see>log.config -> log4net.Appender </see>
/// 
/// MODIFICATION LOG:
/// 
///  Date       By            Notes
/// ---------- ---           -----
/// 2008-10-08 Alert        Initial implementation


using System.Linq;
using System.Text;
using System.Web;
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

namespace Comdiv.MVC.Utils{
    /// <summary>
    /// Служит для логгирования действий пользователя при работе с пользовательским интерфейсом.
    /// Является централизованной точкой логгирования всех действий пользователя, что предоставляет возможность 
    /// включать/выключать логгирование, задавать направление вывода сообщений - file/udp/база данных и т.д.
    /// <see>log.config -> log4net.Appender </see>
    /// </summary>
    public static class UsersActionLogger{
        private static readonly bool isDebugEnabled;
        private static readonly bool isInfoEnabled;
        private static readonly bool isWarnEnabled;

        public static readonly bool isWeb = HttpContext.Current != null;
        private static readonly ILog log = logger.get(typeof (UsersActionLogger));
        private static string m_defaultApplicatonCode = "app";

        static UsersActionLogger(){
            isDebugEnabled = log.IsDebugEnabled;
            isInfoEnabled = log.IsInfoEnabled;
            isWarnEnabled = log.IsWarnEnabled;
        }

        public static string DefaultApplicatonCode{
            get { return m_defaultApplicatonCode; }
            set { m_defaultApplicatonCode = value; }
        }


        private static string AppendUser(string message){
            if (isWeb){
                message = message + "\r\n[USER::" + myapp.principals.CurrentUser.Identity.Name + "]";
            }
            return message;
        }


        /// <summary>
        /// Логирование уровня Debug. Обычно используется для логирования действий пользователя связанных с выборкой чтением данных 
        /// </summary>
        /// <param name="format">формат строка</param>
        /// <param name="objects">параметры для формат-строки</param>
        public static void Debug(string format, params object[] objects){
            if (isDebugEnabled){
                log.Debug(AppendApp(AppendUser(format)), objects);
            }
        }

        private static string AppendApp(string p){
            var sb = new StringBuilder(DefaultApplicatonCode);
            sb.Append(": ");
            sb.Append(p);
            return sb.ToString();
        }


        /// Обычно используется для логирования действий связанных с изменением данных
        public static void Info(string format, params object[] objects){
            if (isInfoEnabled){
                log.Info(AppendApp(AppendUser(format)), objects);
            }
        }

        /// <summary>
        /// Потенциально опасные действия пользователя, например удаление.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="objects"></param>
        public static void Warn(string format, params object[] objects){
            if (isWarnEnabled){
                log.Warn(AppendApp(AppendUser(format)), objects);
            }
        }
    }
}