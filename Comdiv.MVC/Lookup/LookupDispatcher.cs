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
    /// Источник - коллекция и диспетчер других источников, опирается не на источники
    /// напрямую а на ILookupSourceProvider - основной класс общего распределения запросов
    /// подстановки, используется статическим LookupHelper для выполнения запросов
    /// </summary>
    public class LookupDispatcher : ILookupSource, ILookupSourceProvider, IReloadAble{
        //коллекция используется только в защищенном контексте, так как необходимо отслеживать все изменения и операции с ней
        protected IList<ILookupSourceProvider> providers = new List<ILookupSourceProvider>();
        private IEnumerable<ILookupSource> sources;

        #region ILookupSource Members

        /// <summary>
        /// Заглушка псевдонима - диспетчер не является реальным источником
        /// </summary>
        string ILookupSource.Alias{
            get { return "dispatcher"; }
        }

        /// <summary>
        /// Заглушка комментария - диспетчер не является реальным источником
        /// </summary>
        string ILookupSource.Comment{
            get { return "internal system defined collection based source"; }
        }

        /// <summary>
        /// Выполнения запроса подстановки по логике - найти первый источник
        /// соответствующий заданному в запросе псевдониму
        /// и передать выполнение ему
        /// </summary>
        /// <exception cref="AliasNotRegesteredException">Псевдоним query.Alias не обнаружен среди источников ни у одного провайдера</exception>
        /// <param name="query">стандартный запрос подстановки</param>
        /// <returns>результат подстановки</returns>
        public IEnumerable<ILookupItem> Select(ILookupQuery query){
            var source = Sources.FirstOrDefault(s => s.Alias == query.Alias);
            if (null == source){
                throw new AliasNotRegesteredException(query.Alias);
            }
            return source.Select(query);
        }

        #endregion

        #region ILookupSourceProvider Members

        /// <summary>
        /// Приоритет - нормальная реализация, так как в качестве ILookupSourceProvider 
        /// диспетчер может использоваться другим диспетчером
        /// </summary>
        public int Priority { get; set; }

        public IEnumerable<ILookupSource> Sources{
            get{
                //перестраивает список при старте и при инвалидизации коллекции
                if (null == sources){
                    sources = new List<ILookupSource>();
                    //облегчение работы с переменной
                    var slist = (IList<ILookupSource>) sources;

                    //обходит провайдеры в порядке убывания приоритетов
                    //более приоритетные источники получаются в начале списка
                    foreach (var provider in providers.OrderByDescending(p => p.Priority)){
                        foreach (var source in provider.Sources){
                            slist.Add(source);
                        }
                    }
                }
                return sources;
            }
        }

        #endregion

        #region IReloadAble Members

        public void Reload(){
            providers.OfType<IReloadAble>().map(r => r.Reload());
        }

        #endregion

        /// <summary>
        /// Форсирует перестроение списка источников
        /// </summary>
        public void Invalidate(){
            //HACK: форсирование перестроения коллекции
            sources = null;
        }

        /// <summary>
        /// Добавить провайдер источников к списку провайдеров, форсирует перестроение
        /// списка источников
        /// <remarks> не забывайте, что приоритет провайдера зависит
        /// не от порядка в коллекции, а от свойства Priority</remarks>
        /// </summary>
        /// <param name="provider"></param>
        public void AddProvider(ILookupSourceProvider provider){
            if (!providers.Contains(provider)){
                providers.Add(provider);
                Invalidate();
            }
        }

        /// <summary>
        /// Удаление провайдера, форсирует перестроение списка источников
        /// </summary>
        /// <param name="provider"></param>
        public void RemoveProvider(ILookupSourceProvider provider){
            if (providers.Contains(provider)){
                providers.Remove(provider);
                Invalidate();
            }
        }

        /// <summary>
        /// Очистка списка провайдеров, очистка списка источников
        /// </summary>
        public void ClearProviders(){
            if (providers.Count != 0){
                providers.Clear();
                Invalidate();
            }
        }
    }
}