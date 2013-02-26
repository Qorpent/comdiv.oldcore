#region

//using Mediatech.Data;

#endregion

using System;
using System.Data;
using System.Data.Common;
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
using Comdiv.Model.Scaffolding;
using Comdiv.MVC.Controllers;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;

namespace Comdiv.Web{

    #region

    #endregion

    [Layout("bdmanager")]
    public class DatabaseController : AdminControllerBase{

        public DatabaseController(){
            this.storage = myapp.storage.GetDefault();
        }

        //REFACTOR: Какая-то неразбериха с мэпингом папок - путаны ссылки и имена файлов, в итоге всякие out параметры и проч
        private static ILog log = logger.get(typeof (DatabaseController));
        private string exportFileDirectory = "/content/dbscripts/export/";
        private string exportFileNameMask = "{0}.xml";
        private string exportStoredProcedureName = "ioc.getexport";
        private string importFileName = "database.import.file";
        private string importStoredProcedureName = "ioc.getimport";
        private string scriptFileNameMask = "~/content/dbscripts/{0}.{1}.{2}.sql";

        public string ImportFileName{
            get { return importFileName; }
            set { importFileName = value; }
        }

        public string ImportStoredProcedureName{
            get { return importStoredProcedureName; }
            set { importStoredProcedureName = value; }
        }

        public string ExportStoredProcedureName{
            get { return exportStoredProcedureName; }
            set { exportStoredProcedureName = value; }
        }

        public string ScriptFileNameMask{
            get { return scriptFileNameMask; }
            set { scriptFileNameMask = value; }
        }

        public string ExportFileNameMask{
            get { return exportFileNameMask; }
            set { exportFileNameMask = value; }
        }

        public string ExportFileDirectory{
            get { return exportFileDirectory; }
            set { exportFileDirectory = value; }
        }

        public string PreRecreateProcedure { get; set; }

        public string PostRecreateProcedure { get; set; }


        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.NonCritical,
            "Корень консоли администрирвания БД"
            )]
        public void Index() {}


        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.DataAccess,
            "Возвращает текст скрипта формирования базы данных"
            )]
        public void CreationScript(string config){
            var cp = Container.get<IConfigurationProvider>();
            var sfp = Container.get<ISessionFactoryProvider>();
            Configuration cfg = null;
            if (config.noContent()){
                cfg = cp.Get(null);
            }
            else{
                cfg = cp.Get(config);
            }
            var con = sfp.Get(config).OpenSession().Connection;
            using (var con3 = con.GetType().create<IDbConnection>()){
                var con2 = con3;
                if (!(con2 is DbConnection)){
                    con2 = new StubConnection(con2);
                }
                con2.ConnectionString = con.ConnectionString;
                con2.Open();

                Dialect dialect = new MsSql2005Dialect();
                if (con.ConnectionString.ToLower().Contains("postgres")){
                    dialect = new PostgreSQL82Dialect();
                }
                if (con.ConnectionString.ToLower().Contains("oracle")){
                    dialect = new Oracle9iDialect();
                }
                var commands = cfg.GenerateSchemaUpdateScript(
                    dialect, new DatabaseMetadata((DbConnection) con2, dialect));
                SetupScript(commands);
            }
        }


        private void SetupScript(string[] script){
            if (null == script || script.Length == 0){
                PropertyBag["script"] = "<h1>Текущая схема БД полностью соответствует приложению</h1>";
            }
            else{
                PropertyBag["script"] =
                    script.Select(s => s.toHtml() + "<br/>GO<br/>").Aggregate((s1, s2) => s1 + s2);
            }
        }


        private string GetNowFileString(){
            return DateTime.Now.ToString("ioc.getss");
        }


        //FIXED: TODO Оказалось имена перекрываются - надо развести

        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.AppHang,
            "Выполняет запрос в формате HQL"
            )]
        public void Hql(string query, string type){
            
            SetupQuery(query);
            if (query.hasContent()){
                PropertyBag["items"] = storage.Query(query);
            }
        }

        private void SetupQuery(string query){
            PropertyBag["query"] = query;
        }

        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.Dangerous,
            "Выполняет любой запрос в формате SQL"
            )]
        public void Sql(string query){
            SetupQuery(query);
            if (query.hasContent()){
                PropertyBag["dataset"] = Container.getConnection().GetTables(query, null,null);
            }
        }
    }
}