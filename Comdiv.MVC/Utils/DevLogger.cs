///============================================================================
/// TITLE: DevLogger.cs
///
/// CONTENTS:
///
/// Для логгирования.
/// Является централизованной точкой логгирования всех действий приложения, что позволяет
/// включать/выключать логгирование, задавать направление вывода сообщений - file/udp/база данных и т.д.
/// <see>log.config -> log4net.Appender </see>
/// Обычно используется при помощи Windsor Interceptor, и вызов логгера впрыскивается в код
/// <see>Windsor Intercepter</see>
/// 
/// Соглашение по логгированию:
///	DevLogger.Info("cm: Comdiv.MVC.Security.Autorizer: Autorize(string key, bool defaultValue, MvcContext descriptor) with \r\n[{0}, {1},{2}]", key, defaultValue, descriptor);
/// 
/// 1) Указать тип приложения [DevLogger.DefaultApplicatonCode] например сокращенно cm 
/// 2) указать класс Comdiv.MVC.Security.Autorizer
/// 3) Имя метода и его сигнатура 
/// 4) with 
/// 5) написать передаваемые параметры обрамленные в []
/// 6) , the result is 
/// 7) описать возвращаемые значения
/// 
/// MODIFICATION LOG:
/// 
///  Date       By            Notes
/// ---------- ---           -----
/// 2008-10-08 Alert        Initial implementation
/// 2008-10-13 Alert        Реализовал GetSafeConnectionString, для вывода в log безопасной строки соединения без пароля и ///							пользователя
/// 2008-10-16 Alert		Определил соглашение по логу, и поменял шаблоны вывода сообщений в соответствии с соглашением


using System;
using System.Data.SqlClient;
using System.Linq;
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

// REFACTOR: предлагалю если сокращение в три буквы то сокращать как Dal а не DAL. пример Xml. Это обычно так и делается.

namespace Comdiv.MVC.Utils{
    /// <summary>
    /// Служит для логгирования работы Dal уровня.
    /// Является централизованной точкой логгирования всех действий приложения по реализации запросов пользовательского интерфейса, что 
    /// включать/выключать логгирование, задавать направление вывода сообщений - file/udp/база данных и т.д.
    /// <see>log.config -> log4net.Appender </see>
    /// </summary>
    public static class DevLogger{
        private static readonly bool isDebugEnabled;
        private static readonly bool isErrorEnabled;
        private static readonly bool isInfoEnabled;
        private static readonly bool isWarnEnabled;

        public static readonly bool isWeb = HttpContext.Current != null;
        private static readonly ILog log = logger.get(typeof (DevLogger));
        private static string m_defaultApplicatonCode = "app";
        private static string m_defaultExceptionMsg = "Exception";
        public static string m_enteringMethodMsg = "Entering method";
        public static string m_leavingMethodMsg = "Leaving method";
        private static string m_supposeFormat = ". Ошибка может быть вызвана тем, что ";

        static DevLogger(){
            isDebugEnabled = log.IsDebugEnabled;
            isInfoEnabled = log.IsInfoEnabled;
            isWarnEnabled = log.IsWarnEnabled;
            isErrorEnabled = log.IsErrorEnabled;
        }

        public static string DefaultApplicatonCode{
            get { return m_defaultApplicatonCode; }
            set { m_defaultApplicatonCode = value; }
        }

        public static string DefaultExceptionMessage{
            get { return m_defaultExceptionMsg; }
            set { m_defaultExceptionMsg = value; }
        }

        public static string SupposeFormat{
            get { return m_supposeFormat; }
            set { m_supposeFormat = value; }
        }

        public static string EnteringMethodMsg{
            get { return m_enteringMethodMsg; }
        }

        public static string LeavingMethodMsg{
            get { return m_leavingMethodMsg; }
        }

        private static string AppendUser(string message){
            if (isWeb){
                // see UserActionLogger
                message = message + "\r\nby [USER::" + myapp.usrName + "]";
            }
            return message;
        }

        /// <summary>
        /// Лог-сообещение уровня <see cref="log4net.Core.Level.Debug">. Для составления лог-сообщения используется формат-строка и параметры
        /// </summary>
        /// <param name="format">формат строка</param>
        /// <param name="objects">параметры для формат-строки</param>
        public static void Debug(string format, params object[] objects){
            if (isDebugEnabled){
                log.Debug(AppendUser(format), objects);
            }
        }

        public static void Info(string format, params object[] objects){
            if (isInfoEnabled){
                log.Info(AppendUser(format), objects);
            }
        }

        public static void Warn(string format, params object[] objects){
            if (isWarnEnabled){
                log.Warn(AppendUser(format), objects);
            }
        }


        public static void ErorrFormat(string format, params object[] objects){
            if (isErrorEnabled){
                log.Error(AppendUser(format), objects);
            }
        }


        internal static void Warn(Exception ex){
            if (isWarnEnabled){
                log.Warn("", ex);
            }
        }

        internal static void Error(string message){
            if (isErrorEnabled){
                log.Error(message);
            }
        }


        public static void LogExceptions(Exception ex, string message){
            LogExceptions(ex, message, null);
        }

        public static void LogExceptions(Exception ex){
            LogExceptions(ex, null, null);
        }

        /// <summary>
        /// Логирование исключительной ситуации <see cref = "Exception">
        /// </summary>
        /// <param name="ex">Исключение <see cref="Exception"></param>
        /// <param name="message">Сообщение</param>
        /// <param name="suppose">Предположение о причинах возникновения исключительной ситуации</param>
        public static void LogExceptions(Exception ex, string message, string suppose){
            if (log.IsErrorEnabled){
                if (log.IsDebugEnabled){
                    message = message.yes() ? message : DefaultExceptionMessage;
                    message = suppose.yes() ? message + SupposeFormat + suppose : message;
                    log.Debug(message, ex);
                }
                while (ex != null){
                    if (log.IsWarnEnabled){
                        log.Warn("", ex);
                    }
                    if (log.IsErrorEnabled){
                        log.Error(ex.Message);
                    }
                    ex = ex.InnerException;
                }
            }
        }

        public static void Debug(string msg){
            if (log.IsDebugEnabled){
                log.Debug(AppendUser(msg));
            }
        }


        /// <summary>
        /// Лог-сообщение уровня <see cref="log4net.Core.Level.Debug">
        /// </summary>
        /// <param name="msg">Сообщение</param>
        /// <param name="suppose">Предположение о природе сообщения</param>
        /// <param name="solution"></param>
        public static void Debug(string msg, string suppose, string solution){
            if (log.IsDebugEnabled){
                log.Debug(AppendUser(msg));
            }
        }

        public static string GetSafeConnectionString(string connectionString){
            var sb = new SqlConnectionStringBuilder(connectionString);
            sb.Remove("Password");
            sb.Remove("User ID");
            return sb.ToString();
        }
    }
}