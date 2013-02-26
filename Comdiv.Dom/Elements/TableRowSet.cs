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


namespace Comdiv.Dom{
    public class TableRowSet : Node, ITableRowSet{
        internal bool createsHeadersOnDefault;
        private IRowCollection rows;
        public IRow LastAddedRow { get; set; }

        #region ITableRowSet Members

        public void Normalize(){
            ChildrenNodes.Clear();
            foreach (var row in Rows){
                row.Normalize();
                ChildrenNodes.Add(row);
                row.ParentNode = this;
            }
        }

        public IRowCollection Rows{
            get { return rows ?? (rows = new RowCollection{ParentNode = this}); }
        }

        public ITableRowSet Add(IRow row){
            Rows.Add(row);
            LastAddedRow = row;
            return this;
        }

        public ITableRowSet Add(string[] cellValues, params string[] classes){
            var row = getNewRow(classes);
            foreach (var cell in cellValues)
                row.Add(cell);
            return Add(row);
        }

        public ITableRowSet Add(IList<ICell> cells, params string[] classes){
            var row = getNewRow(classes);
            foreach (var cell in cells)
                row.Add(cell);
            return Add(row);
        }

        public IRow this[int rowIndex]{
            get { return Rows[rowIndex]; }
            set { Rows[rowIndex] = value; }
        }

        public ICell this[int rowIndex, int colIndex]{
            get { return this[rowIndex][colIndex]; }
            set { this[rowIndex][colIndex] = value; }
        }

        #endregion

        protected Row getNewRow(string[] classes){
            var row = new Row();
            row.createsHeadersOnDefault = createsHeadersOnDefault;
            BindClasses(classes, row);
            return row;
        }
    }
}