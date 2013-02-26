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


namespace Comdiv.Dom{
    public interface ITable : INode{
        IRow Add(IRow row);
        ITableColumnSet ColumnSet { get; }
        ITableRowSet Head { get; }
        ITableRowSet Body { get; }
        int ColumnCount { get; }
        int HeaderCount { get; }
        int RowsCount { get; }
        void ImportRows(ITable sourceTable);
        void ExtentWithColumns(int count, params string[] columnNames);
        ITable AppendTitle(string title);
        void ImportColumns(ITable sourceTable);
        void ImportColumns(ITable sourceTable, int skipColumnCount);
        ITable Normalize();
        ITable RemoveZeroRows();
        ITable RemoveZeroRows(int skipColumns);
        ITable RemoveZeroRows(int skipRows, int skipColumns);
        ITable RemoveZeroRows(int skipRows, int skipColumns, string[] zeroValues);

        ITable RemoveZeroRows(Func<ITable, int, bool> skipRowPredicate, Func<ITable, int, bool> skipColumnPredicate,
                              Func<ICell, bool> isNullPredicate);

        IRow Add();

    }
    }