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
using System.Diagnostics;
using Comdiv.Extensions;
using Comdiv.QueryEngine;

namespace Comdiv.Persistence{
    ///<summary>
    ///</summary>
    [DebuggerStepThrough] //nothing to debug
    public class StorageQuery : Query<object, StorageQueryContext, StorageQuery>{
        [Obsolete("no use query.current!!!",true)]
        [ThreadStatic]public static StorageQuery Current;
        public string System;

        public QueryDialect Dialect;

        public object Target;

        public StorageQueryType QueryType;

        public Type TargetType;

        public object Key;


        

        private int? _primaryKey;
        public int PrimaryKey{
            get{
                if (_primaryKey == null) _primaryKey = Key.toIntSafe();
                return _primaryKey.Value;
            }
        }

        private string _code = null;
        public string BizCode
        {
            get{
                if (_code == null){
                    if (Key is string){
                        _code = Key as string;
                    }
                    else{
                        _code = String.Empty;
                    }
                }

                return _code;
            }
        }

        public string QueryText;

        public object[] QueryTextPositionalParameters;

        public int MaxCount;

        public int StartIndex;
        public object[] CommonQueryObjects;

        public Type RealType;

        public bool CommandProcessed;

        

        protected override void postToString(System.IO.TextWriter writer)
        {
            writer.WriteLine("command: "+QueryType);
            if (System.hasContent()) writer.WriteLine("system: " + System);
            writer.WriteLine("dialect: " + Dialect);
            if(null!=Target)writer.WriteLine("target: " + Target);
            if(null!=TargetType)writer.WriteLine("type: " + TargetType);
            if(null!=Key) writer.WriteLine("pk: " + Key);
            if(QueryText.hasContent()) writer.WriteLine("qtext: " + QueryText);
            if(QueryTextPositionalParameters.yes())writer.WriteLine("qparams: "+QueryTextPositionalParameters.concat(";"));
            if (CommonQueryObjects.yes()) writer.WriteLine("commons: " + CommonQueryObjects.concat(";"));
            if(MaxCount!=0)writer.WriteLine("max: "+MaxCount);
            if(StartIndex!=0)writer.WriteLine("start: "+StartIndex);
            
            
        }
    }
}
