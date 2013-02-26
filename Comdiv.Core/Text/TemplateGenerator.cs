using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Comdiv.Extensions;

namespace Comdiv.Text
{
    public class TemplateGenerator
    {
        public TemplateGenerator() {
            
        }
        public TemplateGenerator(string template, object datasource=null) {
            this.Template = template;
            this.DataSource = datasource;
        }
        public string Template { get; set; }
        public object DataSource { get; set; }
       public virtual string Generate(object datasource = null) {
           this.DataSource = datasource ?? this.DataSource;
           var result = Template;
           result = result.replace(TemplateGeneratorGlobals.SubstitutionMask,
                                   m =>
                                       {
                                           var code = m.Groups["code"].Value;
                                           var value = getSubstitution(code);
                                           var replace = m.Groups["replace"].Value;
                                           if(replace.hasContent()) {
                                               value = string.Format(replace, value,code);
                                           }
                                           return value;
                                       });
           result = result.replace(TemplateGeneratorGlobals.ConditionalMask,
                                   m =>
                                   {
                                       var code = m.Groups["code"].Value;
                                       var included = isincluded(code);
                                       if (!included) return "";
                                       return m.Groups["content"].Value;
                                   });
           return result;
       }

        protected string getSubstitution(string code) {
            if(DataSource is IDictionary) {
                var res = ((IDictionary) DataSource).get(code, () => "", false, "", typeof (string));
                if(null!=res && (res is bool || res=="True" || res=="False")) {
                    res = res.ToString().ToLower();
                    
                }
                return res.toStr();
            }
           var prop = DataSource.getPropertySafe<object>(code);
           if (null == prop) return "";
           if (prop is ITemplateParameterValue) {
               return ((ITemplateParameterValue) prop).GetValue(code, DataSource, Template);
           }
           if(null!=prop && prop is bool) {
               prop = prop.ToString().ToLower();
           }
           return prop.toStr();
       }
        protected bool isincluded(string code) {
            if (DataSource is IDictionary)
            {
                var val = ((IDictionary)DataSource).get(code, () => false, false, false, typeof(string));
                if(val==null||!(val is string))return val.toBool();
                var vs = val.toStr();
                
                if (vs.ToUpper().isIn("FALSE", "0","")) return false;
                return true;
            }
            var prop = DataSource.getPropertySafe<object>(code);
            if (prop is ITemplateParameterValue)
            {
                return ((ITemplateParameterValue)prop).GetValue(code, DataSource, Template).toBool();
            }
            return prop.toBool();
        }

        public string[] GetSubstitutions() {
            var result = new List<string>();
            foreach (Match m in Regex.Matches(Template,TemplateGeneratorGlobals.SubstitutionMask)) {
                result.Add(m.Groups["code"].Value);
            }
            return result.ToArray();
        }


        public string[] GetConditions() {
            var result = new List<string>();
            foreach (Match m in Regex.Matches(Template, TemplateGeneratorGlobals.ConditionalMask))
            {
                result.Add(m.Groups["code"].Value);
            }
            return result.ToArray();
        }
    }
}
