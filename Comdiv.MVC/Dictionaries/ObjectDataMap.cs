using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Mapping;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Model.Dictionaries{
    internal class ObjectDataMap : ExtendedClassMap<ObjectDataPoco>{
        public ObjectDataMap(string schema)
            : base(schema){
            #if LIB2
            Table("ObjectData");
            #else
            WithTable("ObjectData");
            #endif
            this.standard();
            Map(x => x.TargetType);
            Map(x => x.TargetId);
            References(x => x.DictionaryValue).Standard<ICommonDictionaryValue, DictionaryValuePoco>("DictionaryValue");
        }
    }
}