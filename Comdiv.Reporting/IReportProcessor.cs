// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Collections.Generic;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// Основной интерфейс для работы с отчетной системой - подготовить некий запрос и выполнить
    /// </summary>
    public interface IReportProcessor{
        IMvcContext CallingContext { get; }

        /// <summary>
        /// Выполняет подготовленный запрос
        /// </summary>
        /// <param name="request">Запрос отчета</param>
        void Execute(IReportRequest request);

        /// <summary>
        /// Выполняет запрос по идентифиатору
        /// </summary>
        /// <param name="identity">Ключ идентичности отчета</param>
        /// <param name="advancedParameters">Дополнительные параметры</param>
        /// <returns>Итоговый выполненный запрос</returns>
        IReportRequest Execute(IReportRequestIdentity identity, IDictionary<string, object> advancedParameters);

        /// <summary>
        /// Выполняет именованный запрос
        /// </summary>
        /// <param name="uid">Ключ отчета</param>
        /// <param name="advancedParameters">Дополнительные параметры</param>
        /// <returns></returns>
        IReportRequest Execute(string uid, IDictionary<string, object> advancedParameters);
    }
}