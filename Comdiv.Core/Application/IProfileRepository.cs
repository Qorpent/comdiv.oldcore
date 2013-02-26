using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Comdiv.Application
{
    /// <summary>
    /// Интерфейс хранения данных профиля
    /// </summary>
    public interface IProfileRepository {
        bool IsRealStorage { get; }
        void Save(string name, string content, IPrincipal user = null);
        string Load(string name,  IPrincipal user = null);
        T GetValue<T> (string profile, string propname, IPrincipal user = null);
        void SetValue(string profile, string propname, object value, IPrincipal user = null);
        T Get<T>(string name, T def);
        void Set(string name, object  obj);
        IDictionary<string, object> GetAsDictionary(string profile = null, IPrincipal user = null);
        T Get<T>(string name);
        T GetValue<T> (string profile, string propname);
    }
}
