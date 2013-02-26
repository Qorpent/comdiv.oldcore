#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Dom;
using Comdiv.Extensibility;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Extensions;
using Comdiv.MVC.Scaffolding;
using Comdiv.MVC.Utils;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using NHibernate.Criterion;

#endregion

namespace Comdiv.Web{

    #region

    #endregion

    #region

    #endregion

    /// <summary>
    /// Универсальный контроллер для работы с объектами myapp.storage
    /// В своей работе зависит от наличия службы разрешения псевдонимов и 
    /// передачи параметров классов, для рендеринга приспособлен к 
    /// использованию компонента Scaffolder
    /// </summary>
    [ControllerDetails("object", Area = "database", Sessionless = true)]
    [Filter(ExecuteWhen.Always, typeof (ScriptedFilter), ExecutionOrder = -1000)]
    [AdminAlways]
    public class UniversalObjectManagerController : AdminControllerBase{
        
        private const string defaultNewItemPropertyBag = "default_new_item";
        private const string editObjectPrefix = "edit_object";
        public const string lastInsertedPropertyBag = "last_inserted";
        private const string newObjectPrefix = "new_object";
        private readonly IList<Ref> advansedLinks = new List<Ref>();

        private bool listSelectAllOnNoQuery = true;
        private ILog log = logger.get("comdiv.sys");


        /// <summary>
        /// Признак того, что при отсутствии HQL запроса
        /// выполняется запрос всех объектов
        /// true - запрашивать по умолчанию все объекты
        /// false - не запрашивать ни одного объекта
        /// </summary>
        public bool ListSelectAllOnNoQuery{
            get { return listSelectAllOnNoQuery; }
            set { listSelectAllOnNoQuery = value; }
        }

        public string ItemDesc{
            get { return TargetType.Name + "/" + Id; }
        }

        public IList<Ref> AdvansedLinks{
            get { return advansedLinks; }
        }

        protected internal virtual IEnumerable<Order> GetOrderForList(){
            var result = new List<Order>();
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}list.order.", result);
            return result;
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.DataAccess,
            @"Выводит таблицу объектов БД указанного типа, все параметры берутся из стандартного набора type (alias), hql"
            )]
        public void List(){
            log.debug(() => "uom start");
            Container.getSession().Clear();
            log.debug(() => "sessions cleared");
            //       ExecuteExtenders("^{0}action.list.before.");
            SetupNewItem();
            log.debug(() => "new item prepared");
            PrepareData();
            log.debug(() => "data prepared");
            CommonPrepare();
            log.debug(() => "commons prepared");
            AdvancedPrepare();
            log.debug(() => "advanced prepared");
            if (this.NonEmpty("custom")){
                RenderView(Params["custom"]);
                log.debug(() => "redirected");
            }
            log.debug(() => "CONTROLLER FINISHED");
            //       ExecuteExtenders("^{0}action.list.after.");
        }

        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.DataAccess,
            @"Стандартная обработка автозавершения ввода"
            )]
        public void AcList(string prefix, string custom, bool needNull, bool useCodes){
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}action.aclist.before.");
            PrepareAcData(prefix, needNull);
            PropertyBag["usecodes"] = useCodes;
            if (custom.hasContent()){
                RenderView(custom);
            }
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}action.aclist.after.");
        }


        /// <summary>
        /// Подготовка данных для AutoComplete
        /// </summary>
        /// <param name="prefix">введенная поисковая строка</param>
        private void PrepareAcData(string prefix, bool needNull){
            var hql = string.Empty;
            if ((prefix + HqlParam).hasContent()){
                hql += " where ";
            }
            if (prefix.hasContent()){
                hql += "(x.Name like '%" + prefix + "%') or (x.Code like '" + prefix + "%')";
            }
            if (HqlParam.hasContent()){
                hql += " and (" + HqlParam + ")";
            }
            //Запрос определяется как объединение условия в префиксе и в стандартном параметре HqlParam
            var result = Refactored(Db.Query(TargetType,
                                                        string.Format(
                                                            "from {0} x {1} order by x.Name",
                                                            TargetType.Name,
                                                            hql)
                                        )
                ).Cast<object>().ToList();
            if (needNull){
                var res = new{Name = "NULL", Id = "null", Code = "NULL", Parent = (string) null};
                result.Add(res);
            }
            PropertyBag["items"] = result;
        }


        private void PrepareData(){
            IEnumerable result = new object[]{};

            var sort = TypeAliasHelper.GetParameter(GetAlias(), "sort");
            if (sort.hasContent()){
                sort = " order by " + sort;
            }
            var all = TypeAliasHelper.GetParameter(GetAlias(), "allOnNoHql");
            var mask = TypeAliasHelper.GetParameter(GetAlias(), "hqlmask");
            if (mask.noContent()){
                mask = "from {0} x {1} {2}";
            }
            var hql = GetHql();


            if ("true" == all || all.noContent() || hql.hasContent()){
                result = Db.Query(TargetType,
                                             string.Format(mask, Db.Resolve(TargetType).Name, hql,
                                                           sort)
                    );
            }

            PropertyBag["items"] = Refactored(result);
        }

        private string GetHql(){
            var hql = string.Empty;
            if (HqlParam.hasContent()){
                var myHql = HqlParam;
                if (!myHql.Trim().StartsWith("x.")){
                    var myHql1 = myHql;
                    var parser = Container.all<IUniversalObjectControllerHqlParser>()
                        .Where(p => p.CanParse(TargetType, myHql1)).FirstOrDefault();
                    if (parser.yes()){
                        myHql = parser.Parse(myHql);
                    }
                    else{
                        myHql = "x.Name like '%" + myHql + "%'";
                    }
                }

                hql = " where " + myHql;
            }
            return hql;
        }

        protected void CommonPrepare(){
            var title = TypeAliasHelper.GetParameter(GetAlias(), "title");
            if (title.noContent()){
                title = GetAlias();
            }
            PropertyBag["title"] = title;
            PropertyBag["fixer"] = null;
            PropertyBag["usr"] = myapp.usrName;
            PropertyBag["alias"] = GetAlias();
            PropertyBag["hql"] = HqlParam;
            PropertyBag["targetType"] = TargetType;
            PropertyBag["links"] = AdvansedLinks;
        }

        private void AdvancedPrepare(){
            var refactorer = GetTargetTypeObject("advancedPrepare") as IObjectRefactorer;
            if (null != refactorer){
                refactorer.Refactor(this);
            }
        }

        protected virtual IEnumerable Refactored(IEnumerable source){
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}action.refactorresultlist.", source);
            return source;
        }

        [Cache(HttpCacheability.NoCache)]
        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.DataLoss,
            @"Удаление объекта из БД"
            )]
        public void Delete(){
            using (var c = new TemporaryTransactionSession()){
                var meta = Container.get<IMetadataHelper>().GetMeta(TargetType, GetAlias());
                if (!meta.AllowDelete){
                    ErrorsController.RedirectToException(Context, this,
                                                         new Exception(
                                                             "Попытка удаления объекта, запрещенного для создания данным пользователем " +
                                                             TargetType.FullName));
                    return;
                }
                CancelView();
                //      ExecuteExtenders("^{0}action.delete.before.");
                //using (var s = new SessionScope()){
                //DROPPED: TODO : Рефакторить и такая обработка нужна еще и в Update
                var obj = Db.Load(TargetType, Id);
                if (null == obj){
                    var template =
                        "Невозможно удалить объект типа {0} c идентификатором {1}\r\nобъект уже отсутствует в базе";

                    Response.Write(String.Format(template, TargetType.Name, Id));
                    MVCLog.Write(this, "ioc.geterror", null, ItemDesc);
                    return;
                }
                MVCLog.Write(this, "ioc.getstart", null, ItemDesc);

                try{
                    if (getIsValid("delete", obj)){
                        if (!getCustomPerformed("delete", obj)){
                            //TODO: Перенести этот блок преудаления в персистер
                            if (obj is IWithHooks){
                                ((IWithHooks) obj).PreDelete();
                            }
                            Db.Delete(obj);
                            //c.SessionWrapper.Session.Delete(obj);
                            MVCLog.Write(this, "ioc.getend", null, ItemDesc);
                        }
                        c.Commit();
                    }
                }

                catch (Exception e){
                    Response.Write(String.Format("{0} - {1}\r\n{2}", e.GetType().Name, e.Message, e));
                    MVCLog.Write(this, "ioc.geterror", null, ItemDesc);
                }
            }
        }

        private bool getCustomPerformed(string op, object obj){
            var fs = new FilterState();
            ((IWithScriptExtensions) this).ExecuteExtenders(string.Format("^{{0}}action.{0}.custom", op), fs, obj);
            if (fs.Executed){
                if (fs.Exception.yes()){
                    throw fs.Exception;
                }
                if (!fs.ReturnValue){
                    Response.Write(fs.Message);
                }

                return true;
            }
            return false;
        }


        private bool getIsValid(string op, object obj){
            var fs = new FilterState();
            ((IWithScriptExtensions) this).ExecuteExtenders(string.Format("^{{0}}action.{0}.validate", op), fs, obj);
            if (fs.Executed){
                if (fs.Exception.yes()){
                    throw fs.Exception;
                }
                if (!fs.ReturnValue){
                    Response.Write(fs.Message);
                    return false;
                }
            }
            return true;
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.NonCritical,
            @"Обновление строки в таблице"
            )]
        public void Refresh(){
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}action.refresh.before.");
            var target = GetTarget();
            CommonPrepare();
            SetupItem(target);
            ((IWithScriptExtensions) this).ExecuteExtenders("^{0}action.refresh.after.", target);
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.NonCritical,
            @"Добавление объекта в БД"
            )]
        public void Insert(){
            object target = null;
            using (var c = new TemporaryTransactionSession()){
                var meta = Container.get<IMetadataHelper>().GetMeta(TargetType, GetAlias());
                if (!meta.AllowNew){
                    ErrorsController.RedirectToException(Context, this,
                                                         new Exception(
                                                             "Попытка создания объекта, запрещенного для создания данным пользователем " +
                                                             TargetType.FullName));
                    return;
                }
                CancelView();
                logInsertStart();
                target = Db.New(TargetType);
                new DefaultParametersBinder().Bind(target, newObjectPrefix, Params, GetAlias());
                SetupItem(target);
                CommonPrepare();
                try{
                    if (getIsValid("insert", target)){
                        if (!getCustomPerformed("insert", target)){
                            Db.Save(target);
                            Response.Write(((IWithId) target).Id.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                    logInsertEnd();
                    c.Commit();
                }

                catch (Exception ex){
                    logInsertError();
                    Response.Write(ex.ToString());
                }
            }
            ReloadItem(target);
        }

        private void logInsertStart(){
            MVCLog.Write(this, "ioc.getstart", null, ItemDesc);
        }

        private void logInsertError(){
            MVCLog.Write(this, "ioc.geterror", null, ItemDesc);
        }

        private void logInsertEnd(){
            MVCLog.Write(this, "ioc.getend", null, ItemDesc);
        }

        private void doSaveHook(object target){
            if (target is IWithHooks){
                ((IWithHooks) target).PreSave();
            }
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.NonCritical,
            @"Отображение строки в режиме редактирвания"
            )]
        public void Edit(){
            //       ExecuteExtenders("^{0}action.insert.before.");
            CommonPrepare();
            SetupItem(GetTarget());
            //       ExecuteExtenders("^{0}action.insert.before.");
        }

        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.DataLoss,
            @"Обновление данных объекта в БД"
            )]
        public void Update(){
            object target = null;
            using (var c = new TemporaryTransactionSession()){
                var meta = Container.get<IMetadataHelper>().GetMeta(TargetType, GetAlias());
                if (!meta.AllowEdit){
                    ErrorsController.RedirectToException(Context, this,
                                                         new Exception(
                                                             "Попытка обновления объекта, запрещенного для создания данным пользователем " +
                                                             TargetType.FullName));
                    return;
                }
                MVCLog.Write(this, "ioc.getstart", null, ItemDesc);
                target = GetTarget();
                new DefaultParametersBinder().Bind(target, editObjectPrefix, Params, GetAlias());

                CommonPrepare();
                try{
                    if (getIsValid("update", target)){
                        if (!getCustomPerformed("update", target)){
                            Db.Save(target);
                        }
                        MVCLog.Write(this, "ioc.getend", null, ItemDesc);
                        c.Commit();
                    }
                }
                catch (Exception){
                    MVCLog.Write(this, "ioc.geterror", null, ItemDesc);
                    throw;
                }
            }
            ReloadItem(target);
        }

        private void ReloadItem(object target){
            var t2 = Db.Load(target.GetType(), target.Id());
            Db.Refresh(t2);
            SetupItem(t2);
        }

        protected internal virtual void SetupNewItem(){
            object last;
            if (Flash.ContainsKey(lastInsertedPropertyBag)){
                last = Flash[lastInsertedPropertyBag];
            }
            else{
                last = GetInstanceOfTargetType();
            }

            PropertyBag[lastInsertedPropertyBag] = last;

            PropertyBag[defaultNewItemPropertyBag] = RefactorLastInsertedForNewDefault(last);
        }

        protected internal virtual object RefactorLastInsertedForNewDefault(object last){
            var obj = GetTargetTypeObject("newrefactor");
            return null == obj ? last : ((IObjectRefactorer) obj).Refactor(last);
        }
    }

    public interface IUniversalObjectControllerHqlParser{
        bool CanParse(Type targetType, string query);
        string Parse(string query);
    }
}