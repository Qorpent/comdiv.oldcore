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
using System;
using Comdiv.Extensions;


namespace Comdiv.Dom{
    public partial class Table{
        #region ITable Members

        public void ExtentWithColumns(int count, params string[] columnNames){
            for (var i = 0; i < count; i++){
                foreach (var row in Head.Rows){
                    if (0 == Head.Rows.IndexOf(row) && columnNames.Length > i) row.AddHead(columnNames[i]);
                    else row.AddHead(string.Empty);
                }
                foreach (var row in Body.Rows)
                    row.Add(string.Empty);
            }
        }

        public void ImportRows(ITable sourceTable){
            var myCount = ColumnCount;
            var srcCount = sourceTable.ColumnCount;
            if (myCount < srcCount) ExtentWithColumns(srcCount - myCount);
            if (myCount > srcCount) sourceTable.ExtentWithColumns(myCount - srcCount);
            if (sourceTable.Title.hasContent())
                AppendTitle(sourceTable.Title);
            foreach (var row in sourceTable.Body.Rows)
                Body.Rows.Add((row));
        }


        public void ImportColumns(ITable sourceTable){
            ImportColumns(sourceTable, 0);
        }

        public int HeaderCount{
            get { return Head.Rows.Count; }
        }

        public int RowsCount{
            get { return Body.Rows.Count; }
        }


        public void ImportColumns(ITable sourceTable, int skipColumnCount){
            if (sourceTable == null) throw new ArgumentNullException("sourceTable");
            if (skipColumnCount < 0) throw new ArgumentException("skipColumnCount");
            if (HeaderCount != sourceTable.HeaderCount || RowsCount != sourceTable.RowsCount)
                throw new Exception("“аблицы должны иметь одну структуру");

            for (var row = 0; row < HeaderCount; row++){
                var initialCount = sourceTable.Head[row].CellCount;
                for (var col = skipColumnCount; col < initialCount; col++){
                    var c = sourceTable.Head[row, skipColumnCount];
                    if (!c.Classes.Contains("valuta")){
                        Head[row].Cells.Add(c);
                    }

                }
            }
            for (var row = 0; row < RowsCount; row++){
                var initialCount = sourceTable.Body[row].CellCount;
                for (var col = skipColumnCount; col < initialCount; col++)
                    Body[row].Cells.Add(sourceTable.Body[row, skipColumnCount]);
            }
            Normalize();
        }

        #endregion

        public void ExtentWithColumns(params string[] columnNames){
            if (null == columnNames) return;
            ExtentWithColumns(columnNames.Length, columnNames);
        }
    }
}