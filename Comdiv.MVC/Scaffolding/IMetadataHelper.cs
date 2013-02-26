using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Scaffolding;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Scaffolding{
    public interface IMetadataHelper{
        ClassMetadata GetMeta(Type type);
        ClassMetadata GetMeta(Type type, string viewAlias);
        void Reload();
    }
}