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
using System.Collections.Generic;
using System.Linq;
using Comdiv.Dom.Const.Visual.Styles.Border;
using Comdiv.Extensions;

namespace Comdiv.Dom{

    #region

    #endregion

    #region

    #endregion

    public partial class Table : Node, ITable{
        private ITableRowSet body;
        private ITableColumnSet columnSet;
        private ITableRowSet head;
        private TableSpecialVisualEffects renderOpetions;

        public TableSpecialVisualEffects RenderOpetions{
            get { return renderOpetions ?? (renderOpetions = new TableSpecialVisualEffects(this)); }
        }

        #region ITable Members

        public ITable Normalize(){
            Head.Normalize();
            Body.Normalize();
            Attributes["colcount"] = ColumnCount.ToString();
            return this;
        }

        public IRow Add(IRow row){
            Body.Add(row);
            return row;
        }
       
        public IRow Add(){
            return Add(new Row {ParentNode = Body});
        }

        public ITableColumnSet ColumnSet{
            get { return columnSet ?? (columnSet = new TableColumnSet {ParentNode = this, Name = "ColumnSet"}); }
        }

        public ITableRowSet Head{
            get{
                return head ??
                       (head = new TableRowSet {ParentNode = this, Name = "Head", createsHeadersOnDefault = true});
            }
        }

        public ITableRowSet Body{
            get { return body ?? (body = new TableRowSet {ParentNode = this, Name = "Body"}); }
        }

        public int ColumnCount{
            get{
                //на самом деле не так уж просто подсчитать - сколько же колонок в таблице
                //ибо коллекуия Columns вообще может быть не заполнена и надо определять из наличествующих строк
                //с учетом Colspan
                IRow sampleRow = null;
                //строка для примера
                if (Head.Rows.Count != 0){
                    sampleRow = Head.Rows[0];
                }
                    // может взять ее из заголовка ?
                else if (Body.Rows.Count != 0){
                    sampleRow = Body.Rows[0];
                }
                    // хм... нет? ну тогда из основной части...
                else if (ColumnSet.Columns.Count != 0){
                    return ColumnSet.Columns.Count;
                }
                // и там нет...ну тогда доверимся Columns
                if (null == sampleRow){
                    return 0;
                }
                // если ничего нет значит и колонок нет...

                //а если есть....
                int count = 0;
                foreach (ICell cell in sampleRow.Cells){
                    count++;
                    //единичка за каждую ячейку как таковую
                    if (cell.ColSpan > 1){
                        count += cell.ColSpan - 1;
                    }
                    //если выставлен Colspan смотрим сколько ячеек спрятано было еще...
                }
                return count;
            }
        }


        public ITable AppendTitle(string title){
            int colCount = ColumnCount;
            var titleRow = new Row();
            titleRow.Classes.Add("title");
            var titleCell = new Cell {ColSpan = colCount, Text = title};
            titleCell.Styles["text-align"] = "center";
            titleRow.Add(titleCell);
            Body.Add(titleRow);
            return this;
        }

        public ITable RemoveZeroRows(){
            return RemoveZeroRows(2);
        }

        public ITable RemoveZeroRows(int skipColumns){
            return RemoveZeroRows(0, skipColumns);
        }

        public ITable RemoveZeroRows(int skipRows, int skipColumns){
            return RemoveZeroRows(skipRows, skipColumns, new[] {string.Empty, "0", "-"});
        }

        public ITable RemoveZeroRows(int skipRows, int skipColumns, string[] zeroValues){
            if (zeroValues == null){
                throw new ArgumentNullException("zeroValues");
            }

            return RemoveZeroRows(
                (t, r) => r < skipRows,
                (t, c) => c < skipColumns,
                cell => cell.Text.Trim().isIn(zeroValues)
                );
        }

        public ITable RemoveZeroRows(Func<ITable, int, bool> skipRowPredicate,
                                     Func<ITable, int, bool> skipColumnPredicate, Func<ICell, bool> isNullPredicate){
            //performance - exit before params checking on empty table
            if (0 == RowsCount){
                return this;
            }

            if (null == skipRowPredicate){
                skipRowPredicate = (t, r) => false;
            }
            if (null == skipColumnPredicate){
                skipColumnPredicate = (t, c) => false;
            }
            if (null == isNullPredicate){
                isNullPredicate = cell => cell.Text.noContent();
            }

            var rowsToRemove = new List<int>();


            //обходим все строки насчет выявления

            for (int row = 0; row < RowsCount; row ++){
                if (skipRowPredicate(this, row)){
                    continue;
                }
                if (Body[row].CellCount != ColumnCount){
                    continue;
                }
                if (Body[row].Attributes.ContainsKey("istitle")){
                    continue;
                }


                bool hasNoZeroes = false;
                for (int col = 0; col < ColumnCount; col++){
                    if (skipColumnPredicate(this, col)){
                        continue;
                    }
                    if (!isNullPredicate(Body[row, col])){
                        hasNoZeroes = true;
                        break;
                    }
                }
                if (!hasNoZeroes){
                    rowsToRemove.Add(row);
                }
            }


            //удаляем теперь отобранные нулевые строки

            foreach (int row in rowsToRemove.OrderByDescending(i => i)){
                Body.Rows.RemoveAt(row);
            }

            Normalize();

            return this;
        }

        #endregion

        public ITable FillPage(){
            Styles.Add("width", "100%");
            return this;
        }

        #region Nested type: TableSpecialVisualEffects

        public class TableSpecialVisualEffects{
            private readonly ITable myTable;

            public TableSpecialVisualEffects(ITable table){
                myTable = table;
            }

            public bool CollapseBorders{
                get { return myTable.Styles.AreEqual(Collapse.StyleName, Collapse.Collapsed); }
                set { myTable.Styles[Collapse.StyleName] = value ? Collapse.Collapsed : Collapse.NotCollapsed; }
            }
        }

        #endregion
    }
}