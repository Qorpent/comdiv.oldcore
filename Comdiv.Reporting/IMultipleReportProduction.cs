using System.Collections.Generic;


namespace Comdiv.MVC.Report{
    /// <summary>
    /// ��������� ����������� ���������
    /// </summary>
    public interface IMultipleReportProduction : IReportProduction{
        IEnumerable<IReportProduction> Productions { get; }
    }
}