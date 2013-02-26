using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Comdiv.Extensions;
using Comdiv.Framework.Utils;
using Comdiv.Inversion;
using Comdiv.Model;
using Newtonsoft.Json;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.Application {
    public class ProfileRepository: IDefaultProfileRepository,IProfileRepository {
        public ProfileRepository() {
            DefaultProfile = "zetamain";
        }
        protected IList<IProfileRepository> workers;
        public IPrincipal DefaultUser { get; set; }
        protected virtual IList<IProfileRepository> getWorkers() {
            lock (this) {
                if (null == workers) {
                    workers = myapp.ioc.all<IProfileRepository>().ToList();
                    if(!workers.Any(x=>x.IsRealStorage)) {
                        workers.Add(new LocalFileProfileRepository());
                    }
                    workers = workers.OrderBy(x => x.Idx()).ToArray();
                }
                return workers;
            }
        }

        public bool IsRealStorage {
            get { return true; }
        }

        public void Save(string name, string content, IPrincipal user = null) {
            user = user ?? DefaultUser ?? myapp.usr;
            foreach (var profileRepository in getWorkers()) {
                profileRepository.Save(name,content,user);
            }
        }

        public string Load(string name, IPrincipal user = null) {
            string result = null;
            user = user ?? DefaultUser ?? myapp.usr;
            foreach (var profileRepository in getWorkers()) {
                result = profileRepository.Load(name, user);
                if(result.hasContent()) {
                    return result;
                }
            }
            return string.Empty;
        }

        private IDictionary<string, object> cachedDict;

        public T GetValue<T> (string profile, string propname) {
            return GetValue<T>(profile, propname, null);
        }

        public T GetValue<T> (string profile, string propname, IPrincipal user = null) {
            user = user ?? DefaultUser ?? myapp.usr;
            var dict = cachedDict ?? (cachedDict= GetAsDictionary(profile,user));
            return dict.get(propname).to<T>();
        }

        public string DefaultProfile { get; set; }

        //NOTE: обратная совместимость
        public T Get<T>(string name, T def) {
            var result = GetValue<T>(DefaultProfile, name);
            if(Equals(default(T),result)) {
                result = def;
            }
            return result;
        }

        //NOTE: обратная совместимость
        public T Get<T>(string name) {
            return Get<T>(name, default(T));
        }
        //NOTE: обратная совместимость
        public void Set(string name, object  obj)
        {
            this.SetValue(DefaultProfile,name,obj);
        }

        public IDictionary<string, object> GetAsDictionary(string profile = null, IPrincipal user = null) {
            profile = profile ?? DefaultProfile;
            user = user ?? DefaultUser ?? myapp.usr;
            var profilestring = Load(profile,user);
            return profilestring.fromJSON();
        }

        public void SetValue(string profile, string propname, object value, IPrincipal user = null) {
            user = user ?? DefaultUser ?? myapp.usr;
            var dict = cachedDict ?? (cachedDict = GetAsDictionary(profile, user));
            dict[propname] = value;
            var sw = dict.toJSON();
            Save(profile,sw,user);
        }

        public void Clear() {
            cachedDict.Clear();
        }
    }
}