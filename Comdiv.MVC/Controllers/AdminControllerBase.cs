#region

using System;
using System.Collections.Specialized;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Admin;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

#endregion

namespace Comdiv.Web{

    #region

    #endregion

    #region

    #endregion


    public class AdminControllerBase<T> : BaseController
    {
        public IXmlValueApplyer<T> Applyer { get; set; }
        public void apply(string xml)
        {
            Applyer.Apply(xml);
            RenderText("OK");
        }
    }
    [Obsolete]
    public abstract class AdminControllerBase : BaseController{

        public AdminControllerBase(){
            this.storage = myapp.storage.GetDefault();
        }

        private const string noAliasParameter =
            "ƒанный контроллер требует об€зательной передачи псевдонима класса в параметре type";

        private Type targetType;
        protected StorageWrapper<object> storage;

        /// <summary>
        /// ÷елевой тип дл€ управлени€, определ€етс€ по псевдониму,
        /// переданному в параметре type
        /// </summary>
        public Type TargetType{
            get{
                if (null == targetType){
                    var alias = GetAlias();
                    if (alias.noContent()){
                        throw new Exception(noAliasParameter);
                    }
                    try {
                        targetType = TypeAliasHelper.Resolve(alias);
                    }catch {
                        targetType =
                            myapp.ioc.get<IConfigurationProvider>().Get("Default").GetClassMapping(alias).ClassName.
                                toType();
                    }
                }
                return targetType;
            }
        }

        /// <summary>
        /// Ќекий целевой идентификатор, определ€емый из параметра id
        /// </summary>
        public int Id{
            get { return Params["id"].toInt(); }
        }

        public string HqlParam{
            get { return Params["hql"]; }
        }

        protected NameValueCollection Filter(NameValueCollection col, Func<string, bool> predicate){
            var result = new NameValueCollection(col);
            foreach (var s in col.AllKeys){
                if (!predicate(s)){
                    result.Remove(s);
                }
            }
            return result;
        }

        protected void RedirectToList(Func<string, bool> predicate){
            RedirectToAction("list", Filter(Form, predicate));
        }

        protected void RedirectToList(NameValueCollection c){
            RedirectToAction("list", c);
        }

        protected void RedirectToList(){
            RedirectToList(Form);
        }


        protected string GetAlias(){
            return Params["type"];
        }

        protected object GetInstanceOfTargetType(){
            return storage.New(TargetType);
        }

        protected object GetTargetTypeObject(string name){
            var newRefactorTypeName =
                TypeAliasHelper.GetParameter(GetAlias(), name);
            if (newRefactorTypeName.noContent()){
                return null;
            }
            return Type.GetType(newRefactorTypeName, true).
                GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        protected object GetTarget(){
            object target = null;
            if (0 == Id){
                target = storage.New(TargetType);
            }
            else{
                target = storage.Load(TargetType, Id);
                if (target.no()){
                    throw new Exception("Cannot load {0} with Id {1}"._format(TargetType, Id));
                }
            }
            //target.Refresh();
            return target;
        }

        protected void SetupItem(object obj){
            PropertyBag["item"] = obj;
        }
    }
}