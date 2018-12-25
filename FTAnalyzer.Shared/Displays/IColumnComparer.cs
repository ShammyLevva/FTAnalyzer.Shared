using System.Collections.Generic;

namespace FTAnalyzer
{
    public interface IColumnComparer<T>
    {
        IComparer<T> GetComparer(string columnName, bool ascending);
    }
}
