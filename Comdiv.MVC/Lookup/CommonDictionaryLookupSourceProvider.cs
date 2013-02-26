using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Dictionaries;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Model.Lookup{
    public class CommonDictionaryLookupSourceProvider : ILookupSourceProvider, IReloadAble{


        public CommonDictionaryLookupSourceProvider(){
            this.storage = myapp.storage.Get<ICommonDictionary>();
        }

        private IEnumerable<ILookupSource> sources;
        private StorageWrapper<ICommonDictionary> storage;

        #region ILookupSourceProvider Members

        public int Priority { get; set; }

        /// <summary>
        /// ��������� �������� � ������������ ���������� �� �� � �������������� ������� - ������������
        /// </summary>
        public IEnumerable<ILookupSource> Sources{
            get{
                lock (this){
                    //�� ������������ �� ������ ���, ������ � ������ ������ ��� ����� ������ Reload
                    if (null == sources){
                        sources = new List<ILookupSource>();
                        //����� ������� �������� � ���������� (���-�� sources IEnumerable)
                        var slist = (List<ILookupSource>) sources;
                        var dicts = storage.All();
                        foreach (var dict in dicts){
                            var source = new CommonDictionaryLookupSource
                                         {ParentDict = dict, Alias = "0ld_" + dict.Code, Comment = dict.Name};
                            slist.Add(source);
                        }
                    }
                    return sources;
                }
            }
        }

        #endregion

        #region IReloadAble Members

        /// <summary>
        /// ������������ ������� ���������� �� ��
        /// </summary>
        public void Reload(){
            //HACK: ����� -  ��� ��������� ������� Sources �������� ���������
            sources = null;
        }

        #endregion
    }
}