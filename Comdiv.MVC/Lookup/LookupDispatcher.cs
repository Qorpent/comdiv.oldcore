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
    /// �������� - ��������� � ��������� ������ ����������, ��������� �� �� ���������
    /// �������� � �� ILookupSourceProvider - �������� ����� ������ ������������� ��������
    /// �����������, ������������ ����������� LookupHelper ��� ���������� ��������
    /// </summary>
    public class LookupDispatcher : ILookupSource, ILookupSourceProvider, IReloadAble{
        //��������� ������������ ������ � ���������� ���������, ��� ��� ���������� ����������� ��� ��������� � �������� � ���
        protected IList<ILookupSourceProvider> providers = new List<ILookupSourceProvider>();
        private IEnumerable<ILookupSource> sources;

        #region ILookupSource Members

        /// <summary>
        /// �������� ���������� - ��������� �� �������� �������� ����������
        /// </summary>
        string ILookupSource.Alias{
            get { return "dispatcher"; }
        }

        /// <summary>
        /// �������� ����������� - ��������� �� �������� �������� ����������
        /// </summary>
        string ILookupSource.Comment{
            get { return "internal system defined collection based source"; }
        }

        /// <summary>
        /// ���������� ������� ����������� �� ������ - ����� ������ ��������
        /// ��������������� ��������� � ������� ����������
        /// � �������� ���������� ���
        /// </summary>
        /// <exception cref="AliasNotRegesteredException">��������� query.Alias �� ��������� ����� ���������� �� � ������ ����������</exception>
        /// <param name="query">����������� ������ �����������</param>
        /// <returns>��������� �����������</returns>
        public IEnumerable<ILookupItem> Select(ILookupQuery query){
            var source = Sources.FirstOrDefault(s => s.Alias == query.Alias);
            if (null == source){
                throw new AliasNotRegesteredException(query.Alias);
            }
            return source.Select(query);
        }

        #endregion

        #region ILookupSourceProvider Members

        /// <summary>
        /// ��������� - ���������� ����������, ��� ��� � �������� ILookupSourceProvider 
        /// ��������� ����� �������������� ������ �����������
        /// </summary>
        public int Priority { get; set; }

        public IEnumerable<ILookupSource> Sources{
            get{
                //������������� ������ ��� ������ � ��� ������������� ���������
                if (null == sources){
                    sources = new List<ILookupSource>();
                    //���������� ������ � ����������
                    var slist = (IList<ILookupSource>) sources;

                    //������� ���������� � ������� �������� �����������
                    //����� ������������ ��������� ���������� � ������ ������
                    foreach (var provider in providers.OrderByDescending(p => p.Priority)){
                        foreach (var source in provider.Sources){
                            slist.Add(source);
                        }
                    }
                }
                return sources;
            }
        }

        #endregion

        #region IReloadAble Members

        public void Reload(){
            providers.OfType<IReloadAble>().map(r => r.Reload());
        }

        #endregion

        /// <summary>
        /// ��������� ������������ ������ ����������
        /// </summary>
        public void Invalidate(){
            //HACK: ������������ ������������ ���������
            sources = null;
        }

        /// <summary>
        /// �������� ��������� ���������� � ������ �����������, ��������� ������������
        /// ������ ����������
        /// <remarks> �� ���������, ��� ��������� ���������� �������
        /// �� �� ������� � ���������, � �� �������� Priority</remarks>
        /// </summary>
        /// <param name="provider"></param>
        public void AddProvider(ILookupSourceProvider provider){
            if (!providers.Contains(provider)){
                providers.Add(provider);
                Invalidate();
            }
        }

        /// <summary>
        /// �������� ����������, ��������� ������������ ������ ����������
        /// </summary>
        /// <param name="provider"></param>
        public void RemoveProvider(ILookupSourceProvider provider){
            if (providers.Contains(provider)){
                providers.Remove(provider);
                Invalidate();
            }
        }

        /// <summary>
        /// ������� ������ �����������, ������� ������ ����������
        /// </summary>
        public void ClearProviders(){
            if (providers.Count != 0){
                providers.Clear();
                Invalidate();
            }
        }
    }
}