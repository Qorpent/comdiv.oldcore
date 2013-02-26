using System;
using System.Linq;
using System.Runtime.Serialization;
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

namespace Comdiv.Model.Lookup{
    /// <summary>
    /// Запрошен объект подстановки по псевдониму источника, не зарегистрированного в диспетчере
    /// подстановок
    /// </summary>
    public class AliasNotRegesteredException : LookupException{
        public AliasNotRegesteredException(string message, string alias) : base(message){
            Alias = alias;
        }

        public AliasNotRegesteredException(string alias){
            Alias = alias;
        }

        public AliasNotRegesteredException(SerializationInfo info, StreamingContext context, string alias)
            : base(info, context){
            Alias = alias;
        }

        public AliasNotRegesteredException(string message, Exception innerException, string alias)
            : base(message, innerException){
            Alias = alias;
        }

        /// <summary>
        /// Запрошенный и не зарегистрированный псевдоним
        /// </summary>
        public string Alias { get; protected set; }
    }
}