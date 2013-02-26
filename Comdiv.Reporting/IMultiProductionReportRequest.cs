using System.Collections.Generic;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// Пакетный запрос
    /// </summary>
    public interface IMultiProductionReportRequest{
        IEnumerable<IReportRequest> Requests { get; }
    }
}