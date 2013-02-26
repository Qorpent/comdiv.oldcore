using System.Collections.Generic;
using Comdiv.Collections;

namespace Comdiv.Persistence {
    public class SqlBatchResult {
        private IList<Table> _resultSet = new List<Table>();
        public IList<Table> ResultSet {
            get { return _resultSet; }
            
        }

        private IList<string> _log = new List<string>();
        public IList<string> Log {
            get { return _log; }
            
        }
    }
}