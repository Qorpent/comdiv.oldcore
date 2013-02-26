

namespace Comdiv.MVC.Report{
    /// <summary>
    /// ������������ ������
    /// </summary>
    public interface IReportPreProductor{
        IPreProduction Generate(IReportRequest reportRequest);
    }

    /// <summary>
    /// ��������� ���������
    /// </summary>
    public interface IProductionGenerator{
        IReportProduction Generate(IReportRequest request, IPreProduction preProduction);
    }

    /// <summary>
    /// ��������� ����������� ���-�����, ������ ������ 
    /// </summary>
    public interface IPreProduction {}
}