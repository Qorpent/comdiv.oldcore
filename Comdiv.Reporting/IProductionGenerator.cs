

namespace Comdiv.MVC.Report{
    /// <summary>
    /// Препроцессор отчета
    /// </summary>
    public interface IReportPreProductor{
        IPreProduction Generate(IReportRequest reportRequest);
    }

    /// <summary>
    /// Формирует продукцию
    /// </summary>
    public interface IProductionGenerator{
        IReportProduction Generate(IReportRequest request, IPreProduction preProduction);
    }

    /// <summary>
    /// Описывает абстрактный пре-отчет, модель отчета 
    /// </summary>
    public interface IPreProduction {}
}