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

namespace Comdiv.Dom
{
    public class Row : Node, IRow
    {
        private ICellCollection cells;

        internal bool createsHeadersOnDefault;

        public ICell LastAddedCell { get; set; }

        #region IRow Members

        public void Normalize()
        {
            ChildrenNodes.Clear();
            foreach (ICell cell in Cells)
            {
                ChildrenNodes.Add(cell);
                cell.ParentNode = this;
            }
        }

        public ICellCollection Cells
        {
            get { return cells ?? (cells = new CellCollection {ParentNode = this}); }
        }

        public IRow Add(string text)
        {
            return Add(text, (string[]) null);
        }

        public IRow Add(string text, params string[] classes)
        {
            return Add(null, text, classes);
        }

        public IRow Add(string name, string text, string[] classes)
        {
            Cell cell = null;
            if (createsHeadersOnDefault)
                cell = new Header {Name = name, Text = text};
            else
                cell = new Cell {Name = name, Text = text};

            BindClasses(classes, cell);
            return Add(cell);
        }

        public IRow AddHead(string text)
        {
            return AddHead(text, null);
        }

        public IRow AddHead(string text, Action<ICell> advanced)
        {
            var head = new Header {Text = text};
            if (advanced.yes())
                advanced(head);
            return Add(head);
        }

        public int CellCount
        {
            get { return Cells.Count; }
        }

        public ICell this[int colIndex]
        {
            get { return Cells[colIndex]; }
            set { Cells[colIndex] = value; }
        }

        public IRow Add(ICell cell)
        {
            Cells.Add(cell);
            LastAddedCell = cell;
            return this;
        }

        #endregion

        public IRow Add(string text, Action<ICell> constructor)
        {
            var result = new Cell();
            if (constructor.yes()) constructor(result);
            return Add(result);
        }
    }
}