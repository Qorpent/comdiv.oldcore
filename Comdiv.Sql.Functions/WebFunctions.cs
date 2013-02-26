using System.Data.SqlTypes;
using System.Net;
using System.Text;
using Microsoft.SqlServer.Server;



public partial class UserDefinedFunctions {
    [SqlFunction(IsDeterministic = false, SystemDataAccess = SystemDataAccessKind.Read, Name = "web_get")]
    public static SqlString GetHttp(SqlString url)
    {
        if (url.IsNull) return SqlString.Null;
        var client = new WebClient();
        client.Encoding = Encoding.UTF8;
        return client.DownloadString(url.Value);
    }

    [SqlFunction(IsDeterministic = false, SystemDataAccess = SystemDataAccessKind.Read, Name = "web_get_auth")]
    public static SqlString GetHttpWithAuth(SqlString url,SqlString name, SqlString pass)
    {
        if (url.IsNull) return SqlString.Null;
        var client = new WebClient();
        client.Encoding = Encoding.UTF8;
        if(name.IsNull) client.UseDefaultCredentials = true;
        else {
            client.Credentials = new NetworkCredential(name.Value, pass.Value);
        }
        return client.DownloadString(url.Value);
    }
}