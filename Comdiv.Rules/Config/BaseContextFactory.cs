using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv;
using Comdiv.Extensions;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Config{
    public class BaseContextFactory : IContextFactory{
        private IList<ServiceDescriptor> contextServices = new List<ServiceDescriptor>();

        public IList<ServiceDescriptor> ContextServices{
            get { return contextServices; }
            set { contextServices = value; }
        }

        public string DefaultContextTypeName{
            get{
                if (null == DefaultContextType){
                    return null;
                }
                return DefaultContextType.AssemblyQualifiedName;
            }
            set{
                if (string.IsNullOrEmpty(value)){
                    DefaultContextType = null;
                }
                DefaultContextType = Type.GetType(value, true);
            }
        }

        public Type DefaultContextType { get; set; }

        public IList<IContextDataImporter> Importers { get; set; }

        public IList<IContextDataExporter> Exporters { get; set; }

        #region IContextFactory Members

        public IRuleContext CreateEmptyContext(){
            //		"DefaultContextType".contract_NotNull(DefaultContextType,
            //		                                      "Целевой тип контекста должен быть установлен до вызова конструктора");
            var context = DefaultContextType.create<IRuleContext>();
            //		"context".contract_NotNull(context, "Не удалось инстанцировать базовый класс");


            if (context is IWithServices){
                foreach (var service in ContextServices){
                    service.RegisterService(((IWithServices) context).Services);
                }
            }

            //Загрузка неких данных по умолчанию, если есть дефолтный импортер, срабатывающий на Null, он и обработает
            ImportData(context, null);
            return context;
        }

        public void ImportData(IRuleContext context, object someData){
            foreach (var importer in Importers){
                importer.ImportData(context, someData);
            }
        }

        public object ExportData(IRuleContext context, object dataTargetDescriptor){
            object result = null;
            foreach (var exporter in Exporters){
                result = exporter.ExportData(context, dataTargetDescriptor);
                if (null != result){
                    break;
                }
            }
            return result;
        }

        #endregion
    }
}