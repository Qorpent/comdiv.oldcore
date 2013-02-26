using System.Collections.Generic;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Model{
    public interface IAttachmentRepository<T>
    {
        IAttachment Define(T target, string code, string name, string content,  params byte[] binary);
        void Remove(T target, string attachmentCode);
        IEnumerable<IAttachment> List(T target);
    }

    
}