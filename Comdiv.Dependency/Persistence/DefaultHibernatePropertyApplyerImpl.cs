using System;
using System.Reflection;
using Comdiv.Extensions;

namespace Comdiv.Persistence {
    public class DefaultHibernatePropertyApplyerImpl: HibernatePropertyApplyerImplBase {
        public DefaultHibernatePropertyApplyerImpl() {
            this.Idx = 100000;
        }

        protected override void internalApply(object item, string property, string value,string system) {
            
            var prop = item.GetType().GetProperty(property,
                                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if(null==prop) {
                return;
                ;
                //throw new Exception("cannot find property with name "+prop );
            }
            if(prop.PropertyType==typeof(string)) {
                value = value ?? "";
            }
            prop.SetValue(item, value.to(prop.PropertyType,system),null);
        }

        protected override bool internalMatch(object item, string property, string value) {
            return true;
        }
    }
}