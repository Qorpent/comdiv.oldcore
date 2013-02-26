using System;
using System.Collections.Generic;
using System.Text;
using Comdiv.Model.Interfaces;

namespace Comdiv.Persistence
{
    public interface IHibernatePropertyApplyer:IDisposable {
        IHibernatePropertyApplyer WithSystem(string system);
        void Start(string entity, object id);
        void Start(object item);
        void Apply (string property, string value );
        void Apply (string  entity, int id, string property, string value );
        void Commit();
        object Entity { get; set; }
        StorageWrapper<object> Storage { get; }
        Type RealType { get; }
    }

    public interface IHibernatePropertyApplyerImpl:IWithIdx {
        void Apply(object item, string property, string value,string system);
        bool IsMatch(object item, string property, string value);
    }
}
