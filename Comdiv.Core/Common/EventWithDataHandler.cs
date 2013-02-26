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
namespace Comdiv.Common{
    ///<summary>
    ///</summary>
    ///<param name="sender"></param>
    ///<param name="args"></param>
    ///<typeparam name="TSender"></typeparam>
    ///<typeparam name="TData"></typeparam>
    public delegate void EventWithDataHandler<TSender, TData>(TSender sender, EventWithDataArgs<TData> args);
}