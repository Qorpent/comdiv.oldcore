using System;
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
using Comdiv.Model.Scaffolding;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Web{
    public class AutoMetadataProvider : IMetadataProvider{
        #region IMetadataProvider Members

        public AutoMetadataProvider(){
            this.storage = myapp.storage.GetDefault();
        }

        public ClassMetadata GetMetadata(Type type, string alias){
            logger.get("comdiv.sys").debug(() => "was called on " + type);
            if (type.IsInterface){
                var xt = storage.Resolve(type);
                if (null != xt){
                    type = xt;
                }
            }
            var result = new ClassMetadata();
            IList<PropertyMetadata> properties = new List<PropertyMetadata>();
            internalGetProperties(type, properties, alias);
            IList<CommandDesc> commands = new List<CommandDesc>();
            internalGetCommands(type, alias, commands);
            result.Properties = properties.ToArray();
            result.Commands = commands.ToArray();
            logger.get("comdiv.sys").debug(() => "finished " + type + " " + result.Properties.Count() + " properties");
            return result;
        }

        #endregion

        protected virtual void internalGetCommands(Type type, string alias, IList<CommandDesc> commands) {}

        protected virtual void internalGetProperties(Type type, IList<PropertyMetadata> properties, string alias){
            internalGetPropertiesStart(type, properties, alias);

            internalGetAdditionalProperties(type, properties, alias);

            internalGetPropertesRest(type, properties, alias);
        }

        protected virtual void internalGetPropertesRest(Type type, IList<PropertyMetadata> properties, string alias){
            if (typeof (IWithComment).IsAssignableFrom(type)){
                properties.Add(new PropertyMetadata("Comment").SetTitle("Комментарий"));
            }
        }

        protected virtual void internalGetPropertiesStart(Type type, IList<PropertyMetadata> properties, string alias){
            if (typeof (IWithId).IsAssignableFrom(type)){
                properties.Add(new PropertyMetadata("Id", typeof (int), true).SetTitle("Ид"));
            }
            if (typeof (IWithCode).IsAssignableFrom(type)){
                properties.Add(new PropertyMetadata("Code").SetTitle("Код"));
            }
            if (typeof (IWithName).IsAssignableFrom(type)){
                properties.Add(new PropertyMetadata("Name").SetTitle("Название"));
            }
        }

        protected virtual void internalGetAdditionalProperties(Type type, IList<PropertyMetadata> properties,
                                                               string alias){
            processReferences(type, properties);
            processMaps(type, properties);
            processInvesed(type, properties, alias);
        }

        private void processInvesed(Type type, IList<PropertyMetadata> properties, string alias){
            foreach (var propertyProvider in Container.all<IPropertyProvider>()){
                foreach (var property in propertyProvider.GetProperties(type, alias)){
                    properties.Add(property);
                }
            }
        }

        private IInversionContainer _container;
        private StorageWrapper<object> storage;

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

        private void processMaps(Type type, IList<PropertyMetadata> properties){
            foreach (var property in type.GetProperties().Where(p => p.hasAttribute(typeof (MapAttribute)))){
                var r = property.getFirstAttribute<MapAttribute>();
                var name = r.Title;
                if (name.noContent()){
                    name = property.Name;
                }
                var prop = new PropertyMetadata(property.Name, property.PropertyType).SetTitle(name);
                if (property.PropertyType.IsEnum){
                    prop.FixedList = string.Join(",", Enum.GetNames(property.PropertyType));
                }
                properties.Add(prop);
            }
        }

        private void processReferences(Type type, IList<PropertyMetadata> properties){
            foreach (var property in type.GetProperties().Where(p => p.hasAttribute(typeof (RefAttribute)))){
                var r = property.getFirstAttribute<RefAttribute>();
                var a = r.Alias;
                if (a.noContent()){
                    a = storage.Resolve(property.PropertyType).Name;
                }
                PropertyMetadata lookup = null;
                if (r.AutoComplete.noContent() && !r.IsAutoComplete){
                    var luData = string.Format("hql:from {0}", a);
                    if (r.LookupData.hasContent()){
                        luData = r.LookupData;
                    }

                    lookup = new PropertyMetadata(property.Name, property.PropertyType, luData, r.Nullable);
                }
                else{
                    lookup = new PropertyMetadata(property.Name, property.PropertyType);
                    lookup.IsNullable = r.Nullable;
                    lookup.Autocomplete = true;
                    lookup.AutocompleteType = r.AutoCompleteType;
                    lookup.AutocompleteCustom = r.AutoComplete;
                }
                if (r.Title.hasContent()){
                    lookup.SetTitle(r.Title);
                }
                properties.Add(lookup);
            }
        }
    }
}