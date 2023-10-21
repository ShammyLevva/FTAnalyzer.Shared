namespace FTAnalyzer
{
    public class CensusLocationComparer : DefaultCensusComparer
    {

        public int Level { get; }

        public CensusLocationComparer() => Level = FactLocation.PLACE;

        public CensusLocationComparer(int level) => Level = level;

        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            FactLocation l1 = x.CensusLocation;
            FactLocation l2 = y.CensusLocation;
            int comp = l1.CompareTo(l2, Level);
            if (comp == 0) comp = base.Compare(x, y);
            return comp;
        }
    }
}