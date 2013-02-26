using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;


[Serializable]
[SqlUserDefinedAggregate(Format.UserDefined,IsInvariantToDuplicates = true, IsInvariantToNulls = true, IsInvariantToOrder = true, IsNullIfEmpty = true, Name="concat", MaxByteSize = 8000)]
public struct concat:IBinarySerialize {
    private List<string > strings;
    private string joiner;

    

    public void Init()
    {
        strings = new List<string>();
        joiner = ", ";
    }

    public void Accumulate(SqlString Value) {
       
        if(!Value.IsNull) {
            if (!strings.Contains(Value.Value)) {
                strings.Add(Value.Value);
            }
        }
    }

    public void Merge(concat Group)
    {
        foreach (var s in Group.strings) {
            if(!this.strings.Contains(s)) {
                this.strings.Add(s);
            }
        }
    }

    public SqlString Terminate() {
        bool first = true;
        joiner = joiner ?? ", ";
        var result = "";
        strings.Sort();
        foreach (var s in strings) {
            if(!first) result += joiner;
            result += s;
            first = false;
        }
        return result;
    }

    public void Read(BinaryReader r) {
        strings =new List<string>();
        var str = r.ReadString();
        foreach (var s in str.Split('Ё')) {
            strings.Add(s);
        }
    }

    public void Write(BinaryWriter w) {
        bool first = true;
        var result = "";
        foreach (var s in strings)
        {
            if (!first) result += "Ё";
            result += s;
            first = false;
        }
        w.Write(result);
    }
}
