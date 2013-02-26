using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Application {
    /// <summary>
    /// Хранит данные в локальной файловой системе в файлах
    /// </summary>
    public class LocalFileProfileRepository : IProfileRepository {
        #region IProfileRepository Members

        public bool IsRealStorage {
            get { return true; }
        }

        public void Save(string name, string content, IPrincipal user) {
            myapp.files.Write(getPath(name), content);
        }


        public string Load(string name, IPrincipal user) {
            return (myapp.files.Read(getPath(name)));
        }

        public T GetValue<T>(string profile, string propname, IPrincipal user) {
            throw new NotImplementedException();
        }

        public void SetValue(string profile, string propname, object value, IPrincipal user) {
            throw new NotImplementedException();
        }

        public T Get<T>(string name, T def) {
            throw new NotImplementedException();
        }

        public void Set(string name, object obj) {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetAsDictionary(string profile, IPrincipal user) {
            throw new NotImplementedException();
        }

        public T Get<T>(string name) {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string profile, string propname) {
            throw new NotImplementedException();
        }

        #endregion

        private string getPath(string name) {
            if (name.noContent()) {
                name = "default";
            }
            string ext = Path.GetExtension(name).noContent() ? ".data" : "";
            return string.Format("~/profile/{0}/{1}{2}", myapp.usrName.Replace("\\", "_").ToLower(), name, ext);
        }
    }
}