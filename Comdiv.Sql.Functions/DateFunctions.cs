using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;


public partial class UserDefinedFunctions
{
    [SqlFunction(IsDeterministic = true,SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_dayofweek")]
    public static SqlInt32 DayOfWeek(SqlDateTime time) {
        if (time.IsNull) return SqlInt32.Null;
        var numb = (int) time.Value.DayOfWeek;
        if(numb==0) {
            numb = 7;
        }
        return numb;
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_tomonthstart")]
    public static SqlDateTime ToMonthStart(SqlDateTime time)
    {
        if(time.IsNull) return SqlDateTime.Null;
        return new SqlDateTime(time.Value.Year,time.Value.Month,1);
    }
    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_tomonthend")]
    public static SqlDateTime ToMonthEnd(SqlDateTime time)
    {
        if (time.IsNull) return SqlDateTime.Null;
        return new DateTime(time.Value.Year, time.Value.Month+1,1).AddMilliseconds(-100)
            ;
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_todaystart")]
    public static SqlDateTime ToDayStart(SqlDateTime time)
    {
        if (time.IsNull) return SqlDateTime.Null;
        return new DateTime(time.Value.Year, time.Value.Month, time.Value.Day);
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_todayend")]
    public static SqlDateTime ToDayEnd(SqlDateTime time)
    {
        if (time.IsNull) return SqlDateTime.Null;
        return new DateTime(time.Value.Year, time.Value.Month, time.Value.Day).AddDays(1).AddMilliseconds(-100);
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_toweekstart")]
    public static SqlDateTime ToWeekStart(SqlDateTime time)
    {
        if (time.IsNull) return SqlDateTime.Null;
        var day = (int) time.Value.DayOfWeek;
        if (day == 0) day = 7;
        day = day - 1;
        var result = time.Value.AddDays(-day);
        return new DateTime(result.Year, result.Month, result.Day);
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_toweekend")]
    public static SqlDateTime ToWeekEnd(SqlDateTime time)
    {
        if (time.IsNull) return SqlDateTime.Null;
        var day = (int)time.Value.DayOfWeek;
        if (day == 0) day = 7;
        day = 7 - day;
        var result = time.Value.AddDays(day);
        return new DateTime(result.Year, result.Month, result.Day).AddDays(1).AddMilliseconds(-100);

    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_adddays")]
    public static SqlDateTime AddDays(SqlDateTime time,SqlInt32 days)
    {
        if (time.IsNull) return SqlDateTime.Null;
        if (days.IsNull) return SqlDateTime.Null;
        return time.Value.AddDays(days.Value);
    }


    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_addhours")]
    public static SqlDateTime AddHours(SqlDateTime time, SqlInt32 hours)
    {
        if (time.IsNull) return SqlDateTime.Null;
        if (hours.IsNull) return SqlDateTime.Null;
        return time.Value.AddHours(hours.Value);
    }
    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_hour")]
    public static SqlInt32 GetHour(SqlDateTime time)
    {
        if (time.IsNull) return SqlInt32.Null;
        return time.Value.Hour;
    }

    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_minute")]
    public static SqlInt32 GetMunite(SqlDateTime time)
    {
        if (time.IsNull) return SqlInt32.Null;
        return time.Value.Minute;
    }


    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_addminutes")]
    public static SqlDateTime AddMinutes(SqlDateTime time, SqlInt32 minutes)
    {
        if (time.IsNull) return SqlDateTime.Null;
        if (minutes.IsNull) return SqlDateTime.Null;
        return time.Value.AddMinutes(minutes.Value);
    }



    [SqlFunction(IsDeterministic = true, SystemDataAccess = SystemDataAccessKind.Read, Name = "dt_addmonths")]
    public static SqlDateTime AddMonths(SqlDateTime time, SqlInt32 months)
    {
        if (time.IsNull) return SqlDateTime.Null;
        if (months.IsNull) return SqlDateTime.Null;
        return time.Value.AddMonths(months.Value);
    }
};

