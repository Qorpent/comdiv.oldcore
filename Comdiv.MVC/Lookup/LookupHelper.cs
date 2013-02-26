using System.Collections.Generic;
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

namespace Comdiv.Model.Lookup{
    /// <summary>
    /// ����������� ���������� �������� �� ����������� ������ ����������
    /// </summary>
    public static class LookupHelper{
        private static readonly LookupDispatcher dispatcher = new LookupDispatcher();

        /// <summary>
        /// ���� ������������� ������
        /// </summary>
        public static bool Initialized;

        /// <summary>
        /// ��������� ���������� �����������
        /// </summary>
        public static LookupDispatcher Dispatcher{
            get{
                if (!Initialized){
                    Init();
                }
                return dispatcher;
            }
        }

        /// <summary>
        /// ������� �������� - ������ ������ �� ���� �� ������� �������, ����������
        /// ������ �������� �� ������� ����
        /// </summary>
        /// <param name="alias">��������� ��������� �����������</param>
        /// <param name="code">������������� ���</param>
        /// <returns>������ ����������� (� ����� result.Code == code)</returns>
        public static ILookupItem ByCode(string alias, string code){
            // @"alias".ioc.getHasContent(alias);
            // @"code".ioc.getHasContent(code);
            var query = new LookupQuery{Alias = alias, Code = code, First = true};
            return Dispatcher.Select(query).FirstOrDefault();
        }

        /// <summary>
        /// ������� �������� - ������ ������ ������ �� ����� �����
        /// </summary>
        /// <param name="alias">��������� ��������� �����������</param>
        /// <param name="name">����� �����</param>
        /// <returns>��������� �������� ����������� (� ����� result.Name like name)</returns>
        public static IEnumerable<ILookupItem> ByName(string alias, string name){
            // @"alias".ioc.getHasContent(alias);
            // @"code".ioc.getHasContent(name);
            var query = new LookupQuery{Alias = alias, Name = name};
            return Dispatcher.Select(query);
        }

        /// <summary>
        /// ������� �������� - ������ ���� �������� ���������
        /// </summary>
        /// <param name="alias">��������� ��������� �����������</param>
        /// <returns>��� ������� ��������� ���������</returns>
        public static IEnumerable<ILookupItem> All(string alias){
            // @"alias".ioc.getHasContent(alias);
            var query = new LookupQuery{Alias = alias};
            return Dispatcher.Select(query);
        }

        /// <summary>
        /// �������� ��������� �������, ��������������� ILookupSource
        /// </summary>
        /// <param name="query">����������� ������ �����������</param>
        /// <returns>��������� �������� ����������� �� �������</returns>
        public static IEnumerable<ILookupItem> Select(ILookupQuery query){
            //   @"query".ioc.getNotNull(query);
            return Dispatcher.Select(query);
        }

        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(LookupHelper)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        //������ ����� ������ ����� ��� �� ����

        /// <summary>
        /// ������������� - ��������� ���������� ���������� �� IoC �� ���������
        /// �� ���� ILookupSourceProvider
        /// </summary>
        public static void Init(){
            if (Initialized){
                return;
            }
            dispatcher.ClearProviders();
            foreach (var provider in Container.all<ILookupSourceProvider>()){
                dispatcher.AddProvider(provider);
            }
            Initialized = true;
        }
    }
}