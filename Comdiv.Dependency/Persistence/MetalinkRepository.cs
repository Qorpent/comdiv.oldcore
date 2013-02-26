using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Persistence
{
    public class MetalinkRepository {
        
        public void Save(MetalinkRecord record, string system = "Default") {
           using(var connection = myapp.ioc.getConnection(system)) {
               connection.ExecuteNonQuery("exec comdiv.metalink_set",record);  
           }
        }
        public MetalinkRecord[] Search(MetalinkRecord query, string system="Default") {
            MetalinkRecord[] result = null;
            using (var connection = myapp.ioc.getConnection(system)) {
                result = connection.ExecuteOrm<MetalinkRecord>("exec comdiv.metalink_search", query);
            }
            return result;
        }
    }
}
