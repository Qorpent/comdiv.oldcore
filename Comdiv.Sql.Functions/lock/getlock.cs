using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Comdiv.Extensions;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    public class LockRecord {
        public string Identity { get; set; }
        public DateTime Time { get; set; }
    }
    public static object getlock_sync = new object();
    public static IDictionary<string,LockRecord> locks = new Dictionary<string ,LockRecord>();
    [Microsoft.SqlServer.Server.SqlFunction(Name = "lock_get")]
    public static SqlBoolean getlock(SqlString code, SqlString identity)
    {
        lock(getlock_sync) {
            if(locks.ContainsKey(code.Value) && null!=locks[code.Value]) {
                return locks[code.Value].Identity == identity.Value;
            }else {
                locks[code.Value] = new LockRecord {Identity = identity.Value, Time = DateTime.Now};
                return true;
            }
        }
    }
    [Microsoft.SqlServer.Server.SqlFunction(Name = "lock_releaze")]
    public static SqlBoolean releazelock(SqlString code, SqlString identity)
    {
        lock (getlock_sync) {
            if (identity.IsNull || identity.Value.noContent()) {
                locks[code.Value] = null;
                return true;
            }else {
                if(locks.ContainsKey(code.Value) && null!=locks[code.Value]) {
                    if(locks[code.Value].Identity==identity.Value) {
                        locks[code.Value] = null;
                        return true;
                    }else {
                        return false;
                    }
                }else {
                    return true;
                }
            }

        }
    }

    [SqlFunction(Name="lock_enlist", FillRowMethodName = "LockEnlistRow", TableDefinition = "code nvarchar(255), handler nvarchar(255), time datetime")]
    public static IEnumerable LockEnlist() {
        foreach (var lockRecord in locks) {
            yield return lockRecord;
        }
    }

    public static void LockEnlistRow(object obj, out SqlString code, out SqlString handler, out SqlDateTime time) {
        var pair = (KeyValuePair<string, LockRecord>) obj;
        code = pair.Key;
        if (pair.Value != null) {
            handler = pair.Value.Identity;
            time = pair.Value.Time;
        }else {
            handler = SqlString.Null;
            time = SqlDateTime.Null;
        }
    }
};

