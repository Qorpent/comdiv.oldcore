using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using Comdiv.Extensions;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    [SqlFunction(IsDeterministic = true,SystemDataAccess = SystemDataAccessKind.Read, Name = "tag_has")]
    public static SqlBoolean HasTag(SqlString tagstring, SqlString tag) {
        if (tagstring.IsNull || tag.IsNull) return SqlBoolean.Null;
        return TagHelper.Has(tagstring.Value, tag.Value);
    }
    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "tag_set")]
    public static SqlString SetTag(SqlString tagstring, SqlString tag, SqlString value)
    {
        if ( tag.IsNull) return SqlString.Null;
        return TagHelper.SetValue(tagstring.IsNull?"":tagstring.Value, tag.Value,value.IsNull?null:value.Value);
    }
    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "tag_get")]
    public static SqlString GetTag(SqlString tagstring, SqlString tag)
    {
        if (tagstring.IsNull || tag.IsNull) return SqlString.Null;
        return TagHelper.Value(tagstring.Value, tag.Value);
    }
};

