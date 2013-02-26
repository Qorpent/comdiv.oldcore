using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Security {
    public class UserRecordRepository : IUserRecordRepository {
        private IList<IUserRecordRepository> repositories;

        public UserRecordRepository() {
            myapp.OnReload += myapp_OnReload;
        }

        void myapp_OnReload(object sender, Common.EventWithDataArgs<int> args) {
            this.repositories = null;
        }

        public int Idx { get; set; }

        public IUserRecord[] GetAll() {
            lock (this) {

                checkInit();
                IDictionary<string, IUserRecord> records = new Dictionary<string, IUserRecord>();
                foreach (var repository in repositories) {
                    var rr = repository.GetAll();
                    foreach (var record in rr) {
                        if (!records.ContainsKey(record.Login)) {
                            records[record.Login] = record;
                        }
                    }
                }
                return records.Values.ToArray();
            }
        }

        private void checkInit() {
            lock (this) {
                if(null==repositories) {
                    repositories = Enumerable.ToArray<IUserRecordRepository>(myapp.ioc.all<IUserRecordRepository>().OrderBy(x=>x.Idx));
                }
            }
        }

        public IUserRecord[] Search(string loginmask) {
            IDictionary<string, IUserRecord> records = new Dictionary<string, IUserRecord>();
            foreach (var repository in repositories)
            {
                var rr = repository.Search(loginmask);
                foreach (var record in rr)
                {
                    if (!records.ContainsKey(record.Login))
                    {
                        records[record.Login] = record;
                    }
                }
            }
            return records.Values.ToArray();
        }

        public IUserRecord Get(string login) {
            lock (this) {
                checkInit();
                foreach (var repository in repositories) {
                    var result = repository.Get(login);
                    if (null != result) return result;
                }
                return new UserRecord(login);
            }
        }
    }
}