using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensibility;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Scaffolding;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public interface IParametersBinder{
        void Bind(object target, string prefix, NameValueCollection bindData, string alias);
    }

    public class DefaultParametersBinder : IParametersBinder{
        private readonly Interpreter interpreter = new Interpreter();
        public DefaultParametersBinder(){
            this.storage = myapp.storage.GetDefault();
        }
        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        private IMetadataHelper metadataHelper;
        private StorageWrapper<object> storage;

        private IMetadataHelper MetadataHelper{
            get { return metadataHelper ?? Container.get<IMetadataHelper>(); }
            set { metadataHelper = value; }
        }

        #region IParametersBinder Members

        public void Bind(object target, string prefix, NameValueCollection bindData, string alias){
            IDictionary<string, object> bind = new Dictionary<string, object>();
            foreach (var key in bindData.AllKeys.Where(s => s.hasContent() && s.StartsWith(prefix + "."))){
                var newKey = key.Substring(prefix.Length + 1);
                bind[newKey] = bindData[key].Trim();
            }
            Bind(target, bind, alias);
        }

        #endregion

        public void Bind(object target, IDictionary<string, object> bind, string alias){
            lock (this){
                var meta = MetadataHelper.GetMeta(target.GetType(), alias);
                IList<string> processedData = new List<string>();
                foreach (var metadata in meta.Properties.Where(p => p.CustomSeter != null)){
                    if (bind.ContainsKey(metadata.Name)){
                        var str = bind[metadata.Name];
                        var values = new Dictionary<string, object>();
                        values["target"] = target;
                        values["value"] = str;
                        values["meta"] = metadata;
                        interpreter.Eval(values, null, metadata.CustomSeter);
                        processedData.Add(metadata.Name);
                        //DateTime.ParseExact(System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
                foreach (var propertyName in bind.Keys.Where(k => !processedData.Contains(k) && !k.Contains("."))){
                    target.setProperty(propertyName, bind[propertyName]);
                }
                foreach (var refName in bind.Keys.Where(k => !processedData.Contains(k) && k.EndsWith(".Id"))){
                    var propName = refName.Substring(0, refName.Length - 3);
                    var prop = target.GetType().resolveProperty(propName);
                    if (null == prop){
                        continue;
                    }
                    var type = prop.PropertyType;
                    if ("null" != bind[refName].ToString()){
                        var binded = storage.Load(type, int.Parse(bind[refName].ToString()));
                        target.setProperty(propName, binded);
                    }
                    else{
                        target.setProperty(propName, null);
                    }
                }
                foreach (var refName in bind.Keys.Where(k => !processedData.Contains(k) && k.EndsWith(".WasBool"))){
                    var propName = refName.Substring(0, refName.Length - 8);
                    if (bind.ContainsKey(propName)){
                        continue;
                    }
                    target.setProperty(propName, false);
                }
            }
        }
    }
}