namespace FTAnalyzer
{
    public class CensusLocationComparer : DefaultCensusComparer
    {

        public int Level { get; }

        public CensusLocationComparer() => Level = FactLocation.PLACE;

        public CensusLocationComparer(int level) => Level = level;

        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            FactLocation l1 = x.CensusLocation;
            FactLocation l2 = y.CensusLocation;
            int comp = l1.CompareTo(l2, Level);
            if (comp == 0) comp = base.Compare(x, y);
            return comp;
        }
    }
}