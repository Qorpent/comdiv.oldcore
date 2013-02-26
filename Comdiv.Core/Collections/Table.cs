// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Comdiv.Collections{
    public class Table : ITable{
        private readonly IList<IColumn> columns = new List<IColumn>();
        private readonly IList<IRow> rows = new List<IRow>();

        #region ITable Members

        public string Name { get; set; }

        public IList<IRow> Rows{
            get { return rows; }
        }

        public IList<IColumn> Columns{
            get { return columns; }
        }

        #endregion

        public static Table GetTable(IDataReader reader){
            return GetTableSet(reader).FirstOrDefault();
        }

        public static IEnumerable<Table> GetTableSet(IDataReader reader){
            var wasnextresult = true;
            Table t = null;
            while (reader.Read() || (wasnextresult = (reader.NextResult() && reader.Read()))){
                if (wasnextresult){
                    if (null != t) yield return t;
                    t = initNewTable(reader);
                    wasnextresult = false;
                }
                var r = new Row();
                for (var i = 0; i < reader.FieldCount; i++)
                    r.Values.Add(reader[i]);
                t.Rows.Add(r);
            }
            if (null != t) yield return t;
        }

        private static Table initNewTable(IDataReader reader){
            Table t;
            t = new Table{Name = reader.GetSchemaTable().TableName};
            for (var i = 0; i < reader.FieldCount; i++){
                var c = new Column{Title = reader.GetName(i), Type = reader.GetFieldType(i)};
                t.Columns.Add(c);
            }
            return t;
        }
    }
}