using System.Collections.Generic;

namespace FTAnalyzer
{
    public class FactLocationComparer : Comparer<IDisplayLocation>
    {
        public int Level { get; }

        public FactLocationComparer(int level) => Level = level;

        public override int Compare(IDisplayLocation? x, IDisplayLocation? y)
        {
            if (x == null || y == null) return 0;
            return x.CompareTo(y, Level);
        }
    }
}
