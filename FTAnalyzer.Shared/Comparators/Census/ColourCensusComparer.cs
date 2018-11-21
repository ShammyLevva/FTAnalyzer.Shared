using System.Collections.Generic;

namespace FTAnalyzer
{
    public class ColourCensusComparer : Comparer<IDisplayColourCensus>
    {
        public override int Compare(IDisplayColourCensus x, IDisplayColourCensus y)
        {
            return Compare(x as Individual, y as Individual);
        }
    }
}
