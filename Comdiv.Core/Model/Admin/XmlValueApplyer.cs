using System;
using System.Collections.Generic;
using System.Xml.Linq;
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

namespace Comdiv.Model.Admin{
    public class XmlValueApplyer<T> : IXmlValueApplyer<T>{
        
        public XmlValueApplyer()
        {
            Transactional = true;
        }

        protected bool applyersready = false;
        public void Apply(string xml){
            if(!applyersready){
                prepareApplyers();
                applyersready = true;
            }
            ICanBeCommited s = null;
            try{

                if (Transactional){
                    s = myapp.ioc.get<ICanBeCommited>("default.temporary.current.session");
                }
                var x = XElement.Parse(xml);
                foreach (var e in x.Elements("item"))
                {
                    var id = e.attr("id", 0);
                    var prop = e.attr("prop", "");
                    var value = e.Value;
                    applyValue(id, prop, value);
                }
                if(Transactional){
                    s.Commit();
                }
            }
            finally{
                if(null!=s){
                    s.Dispose();
                }
            }
        }

        protected readonly IDictionary<string, IValueApplyer> applyers =new Dictionary<string, IValueApplyer>();

        protected virtual void prepareApplyers(){
            applyers["name"] = new ByNamePropertyValueApplyer();
            applyers["code"] = new ByNamePropertyValueApplyer();
            applyers["idx"] = new ByNamePropertyValueApplyer();
            applyers["fullname"] = new ByNamePropertyValueApplyer();
            applyers["shortname"] = new ByNamePropertyValueApplyer();
            applyers["comment"] = new ByNamePropertyValueApplyer();

        }

        public bool Transactional { get; set; }
        protected void applyValue(int id, string prop, string value){

            var target = myapp.storage.Get<T>().Load(id);
            if (null == target){
                throw new Exception("cannot load {0} with id {1}"._format(typeof (T).Name, id));
            }
            var applyername = rewriteApplyerName(prop);
            var applyer = applyers[applyername];
            if (null == applyer){
                throw new Exception("cannot find applyer {0} for {1}"._format(applyername,typeof(T).Name));
            }
            applyer.Apply(target,prop,value);
            myapp.storage.Get<T>().Save(target);
        }

        protected virtual string rewriteApplyerName(string prop){
            return prop;
        }
    }
}