using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Model;
using Comdiv.Model.Interfaces;


namespace Comdiv.Zeta.Data
{



    public class WithCodeAttachmentRepository:IAttachmentRepository<IWithCode>{
        public IAttachment Define(IWithCode target, string code, string name, string content, params byte[] binary)
        {
            var result = new Attachment();
            result.MediaType = MediaType.File;
            var ext = Path.GetExtension(name);
            if(ext.noContent()){
                ext = ".txt";
            }
            name = Path.GetFileNameWithoutExtension(name);
            if(code.noContent()){
                code = DateTime.Now.ToString("yyyyMMddhhmmss");
            }
            code = (target.Code + "_" + code).toSystemName();
            result.Code = code;
            string dir = GetDir(target);
            var file = dir + code + ext;
            if(null!=binary && 0!=binary.Length){
                myapp.files.Write(file, binary);
            }
            else{
                myapp.files.Write(file,content ?? "");
            }
            
            var desc = myapp.files.Resolve(dir + code + ".description",false);
            
            new XElement("attach",
                new XAttribute("code",code),
                new XAttribute("name",name),
                new XAttribute("type",ext),
                new XAttribute("file",file),
                new XAttribute("usr",myapp.usrName)
                ).Save(desc);

            result.MimeType = ext;
            result.Query = file;

            return result;
        }

        private string GetDir(IWithCode target)
        {
            return "~/usr/content/attachments/" + target.GetType().Name.ToLower()+"/" + target.Code.toSystemName()+"/";
        }

        public void Remove(IWithCode target, string attachmentCode)
        {
            string descf = GetDescriptorName(target, attachmentCode);
            var desc = myapp.files.Read(descf);
            if (null == desc) return;
            var file = XElement.Parse(desc).Attribute("file").Value;
            myapp.files.Delete(file);
            myapp.files.Delete(descf);
        }

        private string GetDescriptorName(IWithCode target, string attachmentCode)
        {
            return GetDir(target) + attachmentCode + ".description";
        }

        public IEnumerable<IAttachment> List(IWithCode target)
        {
            var attachments = myapp.files.ResolveAll(GetDir(target), "*.description");
            var result = new List<IAttachment>();
            foreach (var attachment in attachments){
                var x = XElement.Load(attachment);
               
                    var a = new Attachment();
                    a.Code = x.Attribute("code").Value;
                    a.Name = x.Attribute("name").Value;
                    a.Query = x.Attribute("file").Value;
                a.Usr = x.Attribute("usr").Value;
                    a.CreationDate = File.GetLastWriteTime(attachment);
                a.Size = (int)new FileInfo(myapp.files.Resolve(a.Query)).Length;
                    a.MediaType = MediaType.File;
                    a.MimeType = x.Attribute("type").Value;
                    result.Add(a);
              
            }
            
            return result;
        }
    }
}
