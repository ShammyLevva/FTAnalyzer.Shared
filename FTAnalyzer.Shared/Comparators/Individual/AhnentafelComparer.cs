using System.Collections.Generic;

namespace FTAnalyzer
{
    public class AhnentafelComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual x, IDisplayIndividual y) => x.Ahnentafel.CompareTo(y.Ahnentafel);
    }
}
