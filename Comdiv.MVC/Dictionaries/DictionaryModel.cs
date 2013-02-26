using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using FluentNHibernate;

namespace Comdiv.Model.Dictionaries{
    public class DictionaryModel : PersistenceModel{
        public DictionaryModel(){
            Add(new DictionaryMap("cpcbpm"));
            Add(new DictionaryValueMap("cpcbpm"));
            Add(new ObjectDataMap("cpcbpm"));
        }
    }
}