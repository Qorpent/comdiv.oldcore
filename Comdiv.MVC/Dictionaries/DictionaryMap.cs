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
    internal class DictionaryMap : ExtendedClassMap<DictionaryPoco>{
        public DictionaryMap(string schema) : base(schema){
#if LIB2
            Table("Dictionary");
#else
            WithTable("Dictionary");
#endif
            this.standard();
            HasMany<DictionaryValuePoco>(x => x.Values).Standard("Dictionary");
        }
    }
}