using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Persistence
{
    public class SqlBasedLockObject : LockObjectBase
    {
        public static SqlBasedLockObject Wait(string lockobject, string system = null, string identity = null)
        {
            return new SqlBasedLockObject(lockobject, identity, system, -1, -1, -1, true, true);
        }
        public static SqlBasedLockObject Timeout(string lockobject, int timeout=1000, int retrycount=1, int retryrate=1000, string system=null, string identity=null) {
            return new SqlBasedLockObject(lockobject, identity, system, timeout,retrycount, retryrate, true, true);
        }
        public static SqlBasedLockObject FullControl(string lockobject, string system=null, string identity=null) {
            return new SqlBasedLockObject(lockobject, identity, system, 0, 0, 0, false, false);
        }
        protected SqlBasedLockObject(string lockobject, string identity, string system, int wait, int retrycount, int retryrate, bool emmidiatly, bool throws) : base(lockobject, identity, system, wait, retrycount, retryrate, emmidiatly, throws) {
        }

        IDbConnection getConnection() {
            return myapp.ioc.get<IConnectionsSource>().Get(System).CreateConnection();
        }
        public override bool NativeGet()
        {
            using (var c = getConnection()) {
                c.WellOpen();
                return
                    c.ExecuteScalar<bool>(string.Format("select dbo.lock_get('{0}','{1}')", LockObject, Identity),
                                          (IParametersProvider)null);
            }
        }

        public override bool NativeRelease()
        {
            using (var c = getConnection())
            {
                c.WellOpen();
                return
                    c.ExecuteScalar<bool>(string.Format("select dbo.lock_releaze('{0}','{1}')", LockObject, Identity),
                                          (IParametersProvider)null);
            }
        }
    }
}