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
    internal class DictionaryValueMap : ExtendedClassMap<DictionaryValuePoco>{
        public DictionaryValueMap(string schema)
            : base(schema){
#if LIB2
            Table("DictionaryValue");
#else
            WithTable("DictionaryValue");
#endif
            this.standard();
            Map(x => x.Value, "Val");
            References(x => x.Dictionary).Standard<ICommonDictionary, DictionaryPoco>("Dictionary");
        }
    }
}