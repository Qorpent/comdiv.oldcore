using System;
using System.Collections.Generic;
using System.Text;
using Comdiv.Common;
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
    public interface IAttachment : IWithCode,IWithName,IWithProperties,IWithUsr
    {
        object Target { get; set; }
        MediaType MediaType { get; set; }
        string MimeType { get; set; }
        string Query { get; set; }
        string Content { get; set; }
        DateTime CreationDate { get; set; }
        int Size { get; set; }
    }

    public enum MediaType{
        None,
        File,
        Sql,
        Hql
    }

    public class Attachment:IAttachment{
        public Attachment(){
            this.Properties = new Dictionary<string, object>();
        }
        public virtual object Target { get; set; }
        public virtual string Code { get; set; }
        public virtual MediaType MediaType { get; set; }
        public virtual string MimeType { get; set; }
        public virtual string Name { get; set; }
        public virtual string Query { get; set; }
        public virtual string Content { get; set; }
        public virtual string Usr { get; set; }
        public virtual int Size
        {
            get;
            set;
        }
        public virtual DateTime CreationDate { get; set; }
        public virtual IDictionary<string, object> Properties { get; private set; }
        public virtual byte[] GetBytes()
        {
            if (Content.hasContent()) return Encoding.UTF8.GetBytes(Content);
            if (this.MediaType==MediaType.File){
                return myapp.files.ReadBinary(this.Query);
            }
            throw new NotImplementedException();
        }
        public virtual string GetString()
        {
            if (Content.hasContent()) return Content;
            if (this.MediaType==Model.MediaType.File){
                return myapp.files.Read(this.Query);
            }
            throw new NotImplementedException();

        }
    }
}