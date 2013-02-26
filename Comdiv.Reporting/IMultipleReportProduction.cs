using System.Collections.Generic;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// Описывает комплексную продукцию
    /// </summary>
    public interface IMultipleReportProduction : IReportProduction{
        IEnumerable<IReportProduction> Productions { get; }
    }
}