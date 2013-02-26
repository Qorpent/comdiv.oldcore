using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using NHibernate.Mapping;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Persistence {
    public class HibernatePropertyApplyer : IHibernatePropertyApplyer {

        public HibernatePropertyApplyer() {
            this.System = "Default";
        }

        protected object Current { get; set; }

        public IHibernatePropertyApplyer WithSystem(string system) {
            if(system.noContent()) {
                system = "Default";
            }
            this.System = system;
            return this;
        }

        protected string System { get; set; }

        public void Start(string entity, object id) {
            Start(getItem(entity,id));
        }

        public void Start(object item) {
            checkthis();
            this.Entity = item;
        }

        public StorageWrapper<object> Storage {
            get { return getStorage(this.EntityName); }
        }
        public Type RealType {
            get { return type; }
        }
        private StorageWrapper<object> cachedstorage;
        protected StorageWrapper<object> getStorage(string entity)
        {
            if (null == cachedstorage) {
                var map =
                    Enumerable.FirstOrDefault(myapp.ioc.get<IConfigurationProvider>().Get(this.System).ClassMappings,
                                              x =>
                                                  {
                                                      if (entity.Contains(".")) {
                                                          return x.EntityName.ToLower() == entity.ToLower();
                                                      }
                                                      else {
                                                          return x.EntityName.ToLower().EndsWith("." + entity.ToLower());
                                                      }
                                                  });
                if (null == map) {
                    throw new Exception("Entity " + entity + " not found");
                }
                var type = map.ClassName.toType();
                this.type = type;
                this.storage = myapp.storage.Get(type, System, true);
                cachedstorage =  this.storage;
            }
            return cachedstorage;
        }

        protected object getItem(string entity, object id) {
            this.EntityName = entity;
            var storage = getStorage(entity);
            object item = null;
                item = storage.Load(type, id);
                if(null==item) {
                    item = storage.New(type);
                }    
                if (null == item) {
                    throw new Exception("No item with Id " + id + " found");
                }
            
            this.Entity = item;
            return this.Entity;
        }

        protected string EntityName { get; set; }


        public void Apply (string  entity, int id, string property, string value ) {
            lock(this) {
                checkthis();
                Start(entity, id);
                Apply(property,value);
                Commit();
            }    
        }

        public void Commit() {
			if(Entity is IDatabaseVerificator) {
				((IDatabaseVerificator) Entity).VerifySaving();
			}
            this.storage.Save(Entity);
        }

        public void Apply(string property, string value) {
            var impl = impls.OrderBy(x => x.Idx).FirstOrDefault(x => x.IsMatch(Entity, property, value));
            if (null == impl)
            {
                throw new Exception("No matched IHibernatePropertyApplyerImpl configured");
            }
            impl.Apply(Entity, property, value,this.System);
        }

        IHibernatePropertyApplyerImpl[] impls;
        private Type type;
        private StorageWrapper<object> storage;
        public object Entity { get; set; }

        void checkthis() {
            impls = impls ?? myapp.ioc.all<IHibernatePropertyApplyerImpl>().ToArray();
        }

        public void Dispose() {
            
        }
    }
}