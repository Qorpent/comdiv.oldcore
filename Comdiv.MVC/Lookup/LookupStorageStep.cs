using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class LookupStorage : DefaultStorage{
        public LookupStorage(){
            UseIocOnPipelineConstruction = false;
            LookupStep = new LookupStorageStep();
        }

        public LookupStorageStep LookupStep { get; set; }

        protected override void preparePipeline(){
            base.preparePipeline();
            Pipeline.Add(LookupStep);
        }
    }

    /// <summary>
    /// Класс поддержки работы с ILookupItem в инфраструктуре Comdiv.Model.myapp.storage,
    /// обрабатывает команды Load и Query, работает с любой имплементацией ILookupItem
    /// </summary>
    /// <remarks>Для работы использует LookupHelper уровня приложения, является реальным персистером-обработчиком</remarks>
    public class LookupStorageStep : StorageQueryStep<LookupStorageStep>{
        //TODO: TEST
        public const string KeyMask = @"^(?<alias>\w+):(?<code>[\s\S]+)$";

        public LookupStorageStep(){
            Syncronized = true;
            myapp.OnReload += (s, a) => Reload();
        }

        public void Reload(){
            LookupHelper.Dispatcher.Reload();
        }

        protected override bool internalIsApplyable(StorageQuery query){
            lock (this){
                return typeof (ILookupItem).IsAssignableFrom(query.TargetType);
            }
        }

        protected override bool getSupport()
        {
            return true;
        }

        protected override object getLoad(){
            lock (this){
                var m = Regex.Match(MyQuery.Key.ToString(), KeyMask, RegexOptions.Compiled);
                var alias = m.Groups["alias"].Value;
                var code = m.Groups["code"].Value;
                return LookupHelper.ByCode(alias, code);
            }
        }

        /// <summary>
        /// Выполняет запрос подстановки
        /// </summary>
        /// <remarks>
        /// Поддерживаемые сигнатуры:
        /// 1) экземпляр ILookupQuery в свойстве BaseQuery - прямо приходит в обработку
        /// 2) строка с псевдонимом в качестве BaseQuery, 
        /// AdvancedQuery может содержать параметры в порядке
        /// string code, string name,bool ioc.getmask, bool ioc.getmask, string custom
        /// при обработке Null игнорируется, "хвост" параметров может быть опущен на любом участке
        /// </remarks>
        /// <param name="context">контекст с запросом</param>
        /// <returns>объекты подстановки</returns>
        protected override IEnumerable getQuery(){
            lock (this){
                //обрабатываем вариант прямого запроса
                if (MyQuery.CommonQueryObjects != null && MyQuery.CommonQueryObjects.FirstOrDefault() is ILookupQuery){
                    return LookupHelper.Select((ILookupQuery) MyQuery.CommonQueryObjects.FirstOrDefault());
                }
                //обрабатываем вариант передачи параметров через BaseQuery==Alias и AdvancedQuery
                if (MyQuery.QueryText.hasContent()){
                    var q = new LookupQuery();
                    q.Alias = MyQuery.QueryText;
                    if (MyQuery.MaxCount == 1){
                        q.First = true;
                    }
                    var i = 0;
                    foreach (var o in MyQuery.QueryTextPositionalParameters){
                        switch (i){
                            case 0:
                                q.Code = (o as string);
                                break;
                            case 1:
                                q.Name = (o as string);
                                break;
                            case 2:
                                if (o is bool){
                                    q.CodeMask = (bool) o;
                                }
                                break;
                            case 3:
                                if (o is bool){
                                    q.NameMask = (bool) o;
                                }
                                break;
                            case 4:
                                q.Custom = o as string;
                                break;
                            default:
                                break;
                        }
                        i++;
                    }
                    return LookupHelper.Select(q);
                }
                throw new LookupException("Неизвестный формат запроса");
            }
        }
    }
}