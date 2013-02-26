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

namespace Comdiv.Persistence{
    public enum StorageQueryType{
        None = 0,
        Supports = 1,
        Exists = 2,
        Load = 3,
        Query = 4,
        Save = 5,
        Delete = 6,
        Resolve = 7,
        New = 8,
        Refresh = 9,
        Custom1 = 10,
        Custom2 = 11,
        Custom3 = 12,
        Custom4 = 13,
        Custom5 = 14,

    }
}