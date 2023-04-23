using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DefaultIndividualComparer : Comparer<Individual>
    {
        int Ascending { get; }

        public DefaultIndividualComparer(bool ascending) => Ascending = ascending ? 1 : -1;

        public override int Compare(Individual? x, Individual? y) => 
            Ascending * string.Compare(x.IndividualID, y.IndividualID, System.StringComparison.Ordinal);
    }
}
