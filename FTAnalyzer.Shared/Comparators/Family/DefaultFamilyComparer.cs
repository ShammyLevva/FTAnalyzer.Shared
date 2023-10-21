using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DefaultFamilyComparer : Comparer<IDisplayFamily>
    {

        public override int Compare(IDisplayFamily? x, IDisplayFamily? y) => string.Compare(x.FamilyID, y.FamilyID, System.StringComparison.Ordinal);
    }
}
