///============================================================================
/// TITLE: DevLogger.cs
///
/// CONTENTS:
///
/// ��� ������������.
/// �������� ���������������� ������ ������������ ���� �������� ����������, ��� ���������
/// ��������/��������� ������������, �������� ����������� ������ ��������� - file/udp/���� ������ � �.�.
/// <see>log.config -> log4net.Appender </see>
/// ������ ������������ ��� ������ Windsor Interceptor, � ����� ������� ������������� � ���
/// <see>Windsor Intercepter</see>
/// 
/// ���������� �� ������������:
///	DevLogger.Info("cm: Comdiv.MVC.Security.Autorizer: Autorize(string key, bool defaultValue, MvcContext descriptor) with \r\n[{0}, {1},{2}]", key, defaultValue, descriptor);
/// 
/// 1) ������� ��� ���������� [DevLogger.DefaultApplicatonCode] �������� ���������� cm 
/// 2) ������� ����� Comdiv.MVC.Security.Autorizer
/// 3) ��� ������ � ��� ��������� 
/// 4) with 
/// 5) �������� ������������ ��������� ����������� � []
/// 6) , the result is 
/// 7) ������� ������������ ��������
/// 
/// MODIFICATION LOG:
/// 
///  Date       By            Notes
/// ---------- ---           -----
/// 2008-10-08 Alert        Initial implementation
/// 2008-10-13 Alert        ���������� GetSafeConnectionString, ��� ������ � log ���������� ������ ���������� ��� ������ � ///							������������
/// 2008-10-16 Alert		��������� ���������� �� ����, � ������� ������� ������ ��������� � ������������ � �����������


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

// REFACTOR: ���������� ���� ���������� � ��� ����� �� ��������� ��� Dal � �� DAL. ������ Xml. ��� ������ ��� � ��������.

namespace Comdiv.MVC.Utils{
    /// <summary>
    /// ������ ��� ������������ ������ Dal ������.
    /// �������� ���������������� ������ ������������ ���� �������� ���������� �� ���������� �������� ����������������� ����������, ��� 
    /// ��������/��������� ������������, �������� ����������� ������ ��������� - file/udp/���� ������ � �.�.
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
        private static string m_supposeFormat = ". ������ ����� ���� ������� ���, ��� ";

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
        /// ���-���������� ������ <see cref="log4net.Core.Level.Debug">. ��� ����������� ���-��������� ������������ ������-������ � ���������
        /// </summary>
        /// <param name="format">������ ������</param>
        /// <param name="objects">��������� ��� ������-������</param>
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
        /// ����������� �������������� �������� <see cref = "Exception">
        /// </summary>
        /// <param name="ex">���������� <see cref="Exception"></param>
        /// <param name="message">���������</param>
        /// <param name="suppose">������������� � �������� ������������� �������������� ��������</param>
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
        /// ���-��������� ������ <see cref="log4net.Core.Level.Debug">
        /// </summary>
        /// <param name="msg">���������</param>
        /// <param name="suppose">������������� � ������� ���������</param>
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