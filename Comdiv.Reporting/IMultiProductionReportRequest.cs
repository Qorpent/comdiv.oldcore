using System.Collections.Generic;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// �������� ������
    /// </summary>
    public interface IMultiProductionReportRequest{
        IEnumerable<IReportRequest> Requests { get; }
    }
}