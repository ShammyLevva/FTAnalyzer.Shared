using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DefaultIndividualComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            return string.Compare(x.IndividualID, y.IndividualID, System.StringComparison.Ordinal);
        }
    }
}
