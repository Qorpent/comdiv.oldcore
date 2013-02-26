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
using Comdiv.Reporting;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// Запрос, который обрабатывается отчетной системой
    /// </summary>
    public interface IReportRequest{
        IReportRequestIdentity RequestId { get; set; }
        IDictionary<string, object> Parameters { get; set; }
        IReportDefinition ReportDefinition { get; set; }
        IReportProductionFormat ProductionFormat { get; set; }
        IReportOutputFormat OutputFormat { get; set; }
        IMvcContext CallingContext { get; set; }
    }
}