using System;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Reporting{
    public interface IReportViewGenerator{
        /// <summary>
        /// Должен вернуть контент вида
        /// </summary>
        /// <param name="definition">Целевое определение отчета</param>
        /// <param name="rootdir">Рутовая директория (для хаков - чтобы мог поставить туда subview)</param>
        /// <returns>BRAIL-код</returns>
        string Generate(IReportDefinition definition, string rootdir);
    }

    public class ReportViewGeneratorBase:IReportViewGenerator{
        public string Template { get; set; }
        public string Generate(IReportDefinition definition, string rootdir){
            var result = mainProcess(definition, rootdir);
            result = postProcess(definition, rootdir, result);
            return result;
        }

        protected virtual string postProcess(IReportDefinition definition, string rootdir, string result){
            return result;
        }

        protected virtual string mainProcess(IReportDefinition definition, string rootdir){
            var result = myapp.files.Read(Template);
            result = embedPropertiesAndParameters(result, definition);
            return result;
        }

        protected virtual string embedPropertiesAndParameters(string result, IReportDefinition definition){
            return result.replace(@"\#\#(\w+)", 
                m=>{
                    var name = m.Groups[1].Value;
                    var prop = definition.GetType().resolveProperty(name);
                    if(null!=prop){
                        return prop.GetValue(definition,null).toStr();
                    }
                    var param = definition.TemplateParameters.FirstOrDefault(x => x.Code == name);
                    if(param!=null){
                        return param.Value.toStr();
                    }
                    return "";
                }
                );
        }
    }
}