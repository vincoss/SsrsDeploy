using System.Collections.Generic;


namespace SsrsDeploy.Engine
{
    public interface IReportItemProvider
    {
        IEnumerable<ReportItem> GetItems();
    }
}
