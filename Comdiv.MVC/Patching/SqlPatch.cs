using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Comdiv.Distribution;


namespace Comdiv.MVC.Patching{
    public class SqlPatch : MvcPatch{
        private string connection = "Default";
        private string script;

        public SqlPatch(){
            AutoLoad = true;
            ReInstallable = true;
        }

        public string Connection{
            get { return connection; }
            set { connection = value; }
        }

        public override string Code{
            get{
                if (__Extensions.noContent(base.Code)){
                    Code = Connection + ":" + FileName;
                }
                return base.Code;
            }
            set { base.Code = value; }
        }

        public string FileName { get; set; }

        public string Script{
            get{
                if (__Extensions.noContent(script)){
                    try{
                        script = "";
                        foreach (var file in FileName.split()){
                            script += Manager.FileSystem.Read(file + ".base");
                            script += "\r\nGO\r\n";
                        }
                    }catch(Exception ex){
                        throw new Exception("Ошибка загрузки файла "+FileName,ex);
                    }
                }
                return script;
            }
            set { script = value; }
        }

        public override IList<IPackageInstallTask> Tasks{
            get{
                if (base.Tasks.Count == 0){
                    base.Tasks.Add(new SqlTask(Connection, Script));
                }
                return base.Tasks;
            }
        }
    }
}