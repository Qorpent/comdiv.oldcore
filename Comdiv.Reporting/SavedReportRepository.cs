using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Persistence;
using Comdiv.Security;
using Qorpent.Security;

namespace Comdiv.Reporting
{
    public interface ISavedReportRepository {
        ISavedReport Define(string code, string name, string comment, string reportcode, string tag, bool shared, IDictionary<string,object> parameters,string role);
        StorageWrapper<ISavedReport> Storage { get; set; }
        void Delete(string code);
    }

    public class SavedReportRepository : ISavedReportRepository{

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = ioc.Container;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        private StorageWrapper<ISavedReport> _storage;
        public StorageWrapper<ISavedReport> Storage{
            get{
                lock (this){
                    if(null==_storage){
                        _storage = myapp.storage.Get<ISavedReport>();
                    }
                    return _storage;
                }
            }
            set { _storage = value; }
        }

        private IRoleResolver _roleResolver;

        public IRoleResolver RoleResolver{
            get{
                lock (this){
                    if(null==_roleResolver){
                        _roleResolver = myapp.roles;
                    }
                    return _roleResolver;
                }
            }
            set { _roleResolver = value; }
        }

        public void Delete(string code){
            var report = Storage.Load(code);
            if(null==report){
                return;
            }
            if(!(RoleResolver.IsAdmin()) && !(myapp.usrName.ToUpper()==report.Usr.ToUpper())){
                throw new Exception("Нельзя удалить чужой отчет");
            }
            if(null!=report){
                foreach (var s in report.Parameters){
                    Storage.Delete(s);
                }
                report.Parameters = new List<ISavedReportParameter>();
            } 
            Storage.Delete(report);
        }

        public ISavedReport Define(string code, string name, string comment, string reportcode, string tag, bool shared, IDictionary<string,object> parameters,string role){
            return Define(code, name, comment, myapp.usrName, reportcode, tag,shared, parameters,role);
        }

        public ISavedReport Define(string code, string name, string comment, string usr, string reportcode, string tag, bool shared, IDictionary<string,object> parameters,string role){
            var report = Storage.Load(code);
            if(null!=report){
                if (!(RoleResolver.IsAdmin()) && (usr.ToUpper() != report.Usr.ToUpper())){
                    throw new Exception("Cannot change saved report of another user");
                }
                if(report.ReportCode!=reportcode){
                    throw new Exception("Cannot change target report of saved report");
                }
                
            }
            if (null == report)
            {
                report = Storage.New();
                report.Usr = usr;
                report.ReportCode = reportcode;
                report.Code = code;
               
            }
            report.Shared = shared;
            report.Name = name;
            report.Comment = comment;
            report.Tag = tag;
            report.Role = role;

            Storage.Save(report);
            foreach (var parameter in parameters){
                Define(report, parameter.Key, parameter.Value.toStr());
            }
            return report;
        }
        public ISavedReportParameter Define(ISavedReport savedReport, string name, string value){
            var existed = Storage.First<ISavedReportParameter>("Report = ? and Name = ?", savedReport, name);
            if(null==existed){
                existed = Storage.New<ISavedReportParameter>();
                existed.Report = savedReport;
                existed.Name = name;
            }
            existed.Value = value;
            Storage.Save(existed);
            return existed;
        }
        
    }
}
