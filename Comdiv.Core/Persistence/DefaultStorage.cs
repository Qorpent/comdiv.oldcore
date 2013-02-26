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
using System.Linq;
using Comdiv.Extensions;
using Comdiv.QueryEngine;

namespace Comdiv.Persistence{
    public class DefaultStorage :
        QueryProcessor<StorageQuery, object, StorageQueryContext, StorageQuery, IStorageQueryStep>, IStorage{
        private static int _id;
        private int id;

        public DefaultStorage() : base(){
            id = _id++;
        }

        public override int GetHashCode(){
            return id;
        }

        protected override StorageQuery prepare(StorageQuery query)
        {
            
            if (query.System.noContent())
            {
                query.System = DefaultSystem;
            }
            return query;
        }

        private string _defaultSystem;

        public virtual string DefaultSystem{
            get { return _defaultSystem; }
            set { _defaultSystem = value; }
        }

	    public virtual IQueryable<TEntity> AsQueryable<TEntity>(string system = null) {
		    throw new NotImplementedException();
	    }
    }
}