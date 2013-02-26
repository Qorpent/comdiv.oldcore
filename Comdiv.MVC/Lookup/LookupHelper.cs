using System.Collections.Generic;
using System.Linq;
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

namespace Comdiv.Model.Lookup{
    /// <summary>
    /// Статический обработчик запросов на подстановку уровня приложения
    /// </summary>
    public static class LookupHelper{
        private static readonly LookupDispatcher dispatcher = new LookupDispatcher();

        /// <summary>
        /// Флаг инициализации класса
        /// </summary>
        public static bool Initialized;

        /// <summary>
        /// Диспетчер источников подстановок
        /// </summary>
        public static LookupDispatcher Dispatcher{
            get{
                if (!Initialized){
                    Init();
                }
                return dispatcher;
            }
        }

        /// <summary>
        /// Типовой сценарий - запрос данных по коду во внешнем словаре, возвращает
        /// первое значение по прямому коду
        /// </summary>
        /// <param name="alias">Псевдоним источника подстановки</param>
        /// <param name="code">Запрашиваемый код</param>
        /// <returns>Объект подстановки (в норме result.Code == code)</returns>
        public static ILookupItem ByCode(string alias, string code){
            // @"alias".ioc.getHasContent(alias);
            // @"code".ioc.getHasContent(code);
            var query = new LookupQuery{Alias = alias, Code = code, First = true};
            return Dispatcher.Select(query).FirstOrDefault();
        }

        /// <summary>
        /// Типовой сценарий - запрос набора данных по маске имени
        /// </summary>
        /// <param name="alias">Псевдоним источника подстановки</param>
        /// <param name="name">Маска имени</param>
        /// <returns>Коллекция объектов подстановки (в норме result.Name like name)</returns>
        public static IEnumerable<ILookupItem> ByName(string alias, string name){
            // @"alias".ioc.getHasContent(alias);
            // @"code".ioc.getHasContent(name);
            var query = new LookupQuery{Alias = alias, Name = name};
            return Dispatcher.Select(query);
        }

        /// <summary>
        /// Типовой сценарий - запрос всех значений источника
        /// </summary>
        /// <param name="alias">Псевдоним источника подстановки</param>
        /// <returns>Все объекты заданного источника</returns>
        public static IEnumerable<ILookupItem> All(string alias){
            // @"alias".ioc.getHasContent(alias);
            var query = new LookupQuery{Alias = alias};
            return Dispatcher.Select(query);
        }

        /// <summary>
        /// Основная сигнатура запроса, соответствующая ILookupSource
        /// </summary>
        /// <param name="query">стандартный запрос подстановки</param>
        /// <returns>Коллекция объектов подстановки по запросу</returns>
        public static IEnumerable<ILookupItem> Select(ILookupQuery query){
            //   @"query".ioc.getNotNull(query);
            return Dispatcher.Select(query);
        }

        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(LookupHelper)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        //особой нужды делать здесь ИНВ не вижу

        /// <summary>
        /// Инициализация - наполняет диспетчера источников из IoC по умолчанию
        /// по типу ILookupSourceProvider
        /// </summary>
        public static void Init(){
            if (Initialized){
                return;
            }
            dispatcher.ClearProviders();
            foreach (var provider in Container.all<ILookupSourceProvider>()){
                dispatcher.AddProvider(provider);
            }
            Initialized = true;
        }
    }
}