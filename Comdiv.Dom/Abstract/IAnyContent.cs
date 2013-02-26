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
    public interface IAnyContent : INode{
        INode LastNode { get; }
        IAnyContent Head(string text, int level);
        IAnyContent Head1(string text);
        IAnyContent Head2(string text);
        IAnyContent Head3(string text);
        IAnyContent Line(string text);
        IAnyContent Paragraph(string text);
        IAnyContent Block();
        IAnyContent Break();
        IAnyContent AddText(string text);
        IAnyContent Add(INode node);
        IListNode List();
        IAnyContent Ref(string address, string text);
        IAnyContent Exit();
        ITable Table();
        IPage Page();
    }
}