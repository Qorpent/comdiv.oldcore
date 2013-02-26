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
    /// Позволяет зарегистрировать фильтр на уровне IoC
    /// фильтр привязывается ко всем контроллерам или по маске (регекс) типа
    /// </summary>
    internal class CustomFilterDecscriptorProvider : ICustomFilterDecscriptorProvider{
        public CustomFilterDecscriptorProvider(){
            ExecuteWhen = ExecuteWhen.Always;
        }

        /// <summary>
        /// Регекс-маска целевого контроллера (по умолчанию - все контроллеры соответвуют, пустая строка
        /// </summary>
        public string MatchMask { get; set; }

        /// <summary>
        /// Тип фильтра
        /// </summary>
        public string FilterType { get; set; }

        /// <summary>
        /// Порядок выполнения фильтра
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Точка выполнения фильтра
        /// </summary>
        public ExecuteWhen ExecuteWhen { get; set; }

        #region ICustomFilterDecscriptorProvider Members

        public bool IsMatch(Type type){
            //если нет маски, то тип точно соответствует
            if (MatchMask.noContent()){
                return true;
            }
            return type.AssemblyQualifiedName.like(MatchMask);
        }

        /// <summary>
        /// Возвращает стандартный дескриптор для фильтра FilterType
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public FilterDescriptor[] CollectFilters(Type controllerType){
            return new[]{new FilterDescriptor(FilterType.toType(), ExecuteWhen, Order, null)};
        }

        /// <summary>
        /// Метод не используется
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Service(IMonoRailServices serviceProvider) {}

        #endregion
    }
}