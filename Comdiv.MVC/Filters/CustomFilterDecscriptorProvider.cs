using System;
using System.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Descriptors;
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

namespace Comdiv.MVC.Filters{
    /// <summary>
    /// ��������� ���������������� ������ �� ������ IoC
    /// ������ ������������� �� ���� ������������ ��� �� ����� (������) ����
    /// </summary>
    internal class CustomFilterDecscriptorProvider : ICustomFilterDecscriptorProvider{
        public CustomFilterDecscriptorProvider(){
            ExecuteWhen = ExecuteWhen.Always;
        }

        /// <summary>
        /// ������-����� �������� ����������� (�� ��������� - ��� ����������� �����������, ������ ������
        /// </summary>
        public string MatchMask { get; set; }

        /// <summary>
        /// ��� �������
        /// </summary>
        public string FilterType { get; set; }

        /// <summary>
        /// ������� ���������� �������
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// ����� ���������� �������
        /// </summary>
        public ExecuteWhen ExecuteWhen { get; set; }

        #region ICustomFilterDecscriptorProvider Members

        public bool IsMatch(Type type){
            //���� ��� �����, �� ��� ����� �������������
            if (MatchMask.noContent()){
                return true;
            }
            return type.AssemblyQualifiedName.like(MatchMask);
        }

        /// <summary>
        /// ���������� ����������� ���������� ��� ������� FilterType
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public FilterDescriptor[] CollectFilters(Type controllerType){
            return new[]{new FilterDescriptor(FilterType.toType(), ExecuteWhen, Order, null)};
        }

        /// <summary>
        /// ����� �� ������������
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Service(IMonoRailServices serviceProvider) {}

        #endregion
    }
}