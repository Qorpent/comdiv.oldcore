using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;


public partial class UserDefinedFunctions {
    private static System.Security.Cryptography.MD5 hasher = System.Security.Cryptography.MD5.Create();
    [SqlFunction(IsDeterministic = true,SystemDataAccess=SystemDataAccessKind.None, Name="md5")]
    public static SqlString MD5(SqlString data) {
        if (data.IsNull) return SqlString.Null;
        var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(data.Value));
        return Convert.ToBase64String(hash);
    }
    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.None, Name = "md5x")]
    public static SqlString MD5X(SqlXml data)
    {
        if (data.IsNull) return SqlString.Null;
        var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(data.Value));
        return Convert.ToBase64String(hash);
    }
}


public static partial class UserDefinedFunctions
{
    [SqlFunction(IsDeterministic = true,SystemDataAccess = SystemDataAccessKind.Read, Name = "rx_like")]
    public static SqlBoolean RegexLike(SqlString stringtotest, SqlString regex) {
        if (stringtotest.IsNull) return SqlBoolean.Null;
        if (regex.IsNull) return SqlBoolean.Null;
        return Regex.IsMatch(stringtotest.Value, regex.Value, RegexOptions.Compiled);
    }

    [SqlFunction(IsDeterministic = true,SystemDataAccess = SystemDataAccessKind.Read, Name="rx_match")]
    public static SqlString RegexMatch(SqlString stringtotest, SqlString regex)
    {
        if (stringtotest.IsNull) return SqlString.Null;
        if (regex.IsNull) return SqlString.Null;
        return Regex.Match(stringtotest.Value, regex.Value, RegexOptions.Compiled).Value;
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "str_format")]
    public static SqlString StrFormat(SqlString pattern, SqlString s1, SqlString s2, SqlString s3, SqlString s4, SqlString s5, SqlString s6)
    {
        if (pattern.IsNull) return SqlString.Null;
        return string.Format(pattern.Value,
                             s1.IsNull ? null : s1.Value,
                             s2.IsNull ? null : s2.Value,
                             s3.IsNull ? null : s3.Value,
                             s4.IsNull ? null : s4.Value,                
                             s5.IsNull ? null : s5.Value, 
                             s6.IsNull ? null : s6.Value

            );
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "rx_replace")]
    public static SqlString RegexReplace(SqlString stringtotest, SqlString regex, SqlString replace)
    {
        if (stringtotest.IsNull) return SqlString.Null;
        if (regex.IsNull) return SqlString.Null;
        if (replace.IsNull) return SqlString.Null;
        return Regex.Replace(stringtotest.Value, regex.Value, replace.Value, RegexOptions.Compiled);
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "rx_group")]
    public static SqlString RegexGroup(SqlString stringtotest, SqlString regex, SqlString groupnameoridx)
    {
        if (stringtotest.IsNull) return SqlString.Null;
        if (regex.IsNull) return SqlString.Null;
        if (groupnameoridx.IsNull) return SqlString.Null;
        var m = Regex.Match(stringtotest.Value, regex.Value, RegexOptions.Compiled);
        int idx = 0;
        if(Int32.TryParse(groupnameoridx.Value, out idx)) {
            return m.Groups[idx].Value;
        }else {
            return m.Groups[groupnameoridx.Value].Value;
        }
    }

    [SqlFunction(FillRowMethodName = "FillMatchesRow",Name = "rx_matches",IsDeterministic = true,DataAccess=DataAccessKind.Read,SystemDataAccess =SystemDataAccessKind.Read,
        TableDefinition = "idx int, length int, value nvarchar(max)")]
    public static IEnumerable RegexMatches(SqlString stringtotest, SqlString regex) {


        return Regex.Matches(stringtotest.IsNull ? "" : stringtotest.Value, regex.IsNull ? "" : regex.Value);
    }

    public static void FillMatchesRow(Object obj, out SqlInt32 idx, out SqlInt32 length, out SqlString value)
    {
        Match m = (Match)obj;
        idx = m.Index;
        length = m.Length;
        value = m.Value;
    }
    class MatchRecord {
        public int Idx;
        public int Length;
        public string Value;
        public string GroupValue;
        public string Key;
	    public string this[int idx] {
		    get {
			    if(idx>=GroupValues.Count) return null;
			    return GroupValues[idx];
		    }
			set { GroupValues[idx] = value; }
	    }
		IList<string> GroupValues = new List<String>(new[]{"","","","","","","","","","","","","","","",""});
    }
    [SqlFunction(FillRowMethodName = "FillMatchesGroupRow", Name = "rx_matches_g", IsDeterministic = true, DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read,
       TableDefinition = "idx int, length int, value nvarchar(max), groupvalue nvarchar(max), fkey nvarchar(255)")]
    public static IEnumerable RegexMatchesGroup(SqlString stringtotest, SqlString regex, SqlString groupname, SqlString key)
    {


        foreach (Match m in  Regex.Matches(stringtotest.IsNull ? "" : stringtotest.Value, regex.IsNull ? "" : regex.Value)) {
            yield return
                new MatchRecord
                    {Idx = m.Index, Length = m.Length, Value = m.Value, GroupValue = m.Groups[groupname.Value].Value, Key = key.IsNull?"":key.Value};
        }
    }

	[SqlFunction(FillRowMethodName = "FillMatchesAllGroupsRow", Name = "rx_matches_allg", IsDeterministic = true, DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read,
	  TableDefinition = "idx int, length int, value nvarchar(max), fkey nvarchar(255), g1 nvarchar(255), g2 nvarchar(255), g3 nvarchar(255) , g4 nvarchar(255) , g5 nvarchar(255), g6 nvarchar(255) , g7 nvarchar(255), g8 nvarchar(255), g9 nvarchar(255), g10 nvarchar(255)")]
	public static IEnumerable RegexMatchesAllGroup(SqlString stringtotest, SqlString regex, SqlString key)
	{


		foreach (Match m in Regex.Matches(stringtotest.IsNull ? "" : stringtotest.Value, regex.IsNull ? "" : regex.Value))
		{
			var record = new MatchRecord{ Idx = m.Index, Length = m.Length, Value = m.Value,  Key = key.IsNull ? "" : key.Value };
			for(var i = 1; i<=m.Groups.Count && i<=10;i++) {
				record[i] = m.Groups[i].Value;
			}
			yield return record;
		}
	}
	public static void FillMatchesAllGroupsRow(Object obj, out SqlInt32 idx, out SqlInt32 length, out SqlString value,  out SqlString fkey,
		out SqlString g1, out SqlString g2, out SqlString g3, out SqlString g4, out SqlString g5, 
		out SqlString g6, out SqlString g7, out SqlString g8, out SqlString g9, out SqlString g10
	)
	{
		MatchRecord m = (MatchRecord)obj;
		idx = m.Idx;
		length = m.Length;
		value = m.Value;
		
		fkey = m.Key;
		g1 = m[1];
		g2 = m[2];
		g3 = m[3];
		g4 = m[4];
		g5 = m[5];
		g6 = m[6];
		g7 = m[7];
		g8 = m[8];
		g9 = m[9];
		g10 = m[10];
	}
    public static void FillMatchesGroupRow(Object obj, out SqlInt32 idx, out SqlInt32 length, out SqlString value,out SqlString groupvalue, out SqlString fkey)
    {
        MatchRecord m = (MatchRecord)obj;
        idx = m.Idx;
        length = m.Length;
        value = m.Value;
        groupvalue = m.GroupValue;
        fkey = m.Key;
    }
};

