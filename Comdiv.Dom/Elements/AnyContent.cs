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
namespace Comdiv.Dom{
    public class AnyContent : Node, IAnyContent{
        #region IAnyContent Members

        public IListNode List(){
            IListNode result = new List();
            Add(result);
            return result;
        }

        //HACK: „тобы удобно было выйти из наполнени€
        public IAnyContent Exit(){
            return ParentNode as IAnyContent;
        }

        public IAnyContent Ref(string address, string text){
            return Add(new Ref{Address = address, Text = text});
        }

        public INode LastNode { get; protected set; }

        public IAnyContent Head(string text, int level){
            INode result = new Node{Name = "h" + level, Text = text};
            Add(result);
            return this;
        }

        public IAnyContent Head1(string text){
            return Head(text, 1);
        }

        public IAnyContent Head2(string text){
            return Head(text, 2);
        }

        public IAnyContent Head3(string text){
            return Head(text, 3);
        }

        public IAnyContent Line(string text){
            AddText(text);
            return Break();
        }

        public IAnyContent Paragraph(string text){
            var result = new AnyContent{Name = "p", Text = text};
            Add(result);
            return this;
        }

        public IAnyContent Block(){
            var result = new AnyContent{Name = "Block"};
            Add(result);
            return result;
        }

        public IAnyContent Break(){
            return Add(new Node{Name = "Break"});
        }

        public IAnyContent AddText(string text){
            var result = new Text{Text = text};
            return Add(result);
        }

        public IAnyContent Add(INode node){
            LastNode = node;
            ChildrenNodes.Add(node);
            return this;
        }

        public IPage Page(){
            var result = new Page();
            Add(result);
            return result;
        }

        public ITable Table(){
            var result = new Table();
            Add(result);
            return result;
        }

        #endregion
    }
}