#region

using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Scaffolding;
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
    /// Производит мэпинг псевдонимов типов в целевые типы, для передачи сокращенных
    /// строк в составе параметров.
    /// Мапинг берется из файла ~/data/comdiv.type.aliases.xml из
    /// элементов вида &lt;map name="alias" type="fully specified type referense" /&gt;
    /// </summary>
    public class TypeAliasHelper{
        //  private const string filePath = "~/data/comdiv.type.aliases.xml";
        private const string notFoundErrorMessage = "Type with '{0}' alias not found";

        

        public static Type Resolve(string alias){
            return Resolve(alias, true);
        }

        public static string ResolveName(string alias){
            return ResolveName(alias, true);
        }


        public static Type Resolve(string alias, bool throwExceptionOnNotFoundAlias){
            var result = innerResolveAliase(alias);
            if (null == result && throwExceptionOnNotFoundAlias){
                throw new TypeAliasHelperException(string.Format(notFoundErrorMessage, alias));
            }
            return result;
        }

        public static string ResolveName(string alias, bool throwExceptionOnNotFoundAlias){
            var result = innerResolveName(alias);
            if (null == result && throwExceptionOnNotFoundAlias){
                throw new TypeAliasHelperException(string.Format(notFoundErrorMessage, alias));
            }
            return result;
        }

        public static string GetParameter(string alias, string parameter){
            return innerResolveParameter(alias, parameter);
        }


        protected static string innerResolveName(string alias){
            var result = innerResolveParameter(alias, "type");
            if (result.noContent()){
                return null;
            }
            return result;
        }

        protected static string innerResolveParameter(string alias, string parameter){
            var cls = myapp.storage.Get<ICls>().First(Restrictions.Eq("Code", alias));
            if (null == cls){
                return null;
            }
            if ("type" == parameter.ToLower()){
                return cls.Name;
            }
            if ("title" == parameter.ToLower()){
                return cls.Title;
            }
            var prop = cls.Properties.FirstOrDefault(p => p.ClsPropertyType.Code == parameter);
            if (null == prop){
                return null;
            }
            return prop.Name;
        }

        protected static Type innerResolveAliase(string alias){
            var name = innerResolveName(alias);
            if (null == name){
                return null;
            }
            return Type.GetType(name, true, true);
        }
    }
}