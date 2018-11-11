namespace FTAnalyzer
{
    public class CensusLocationComparer : DefaultCensusComparer
    {
        private int level;
        
        public CensusLocationComparer() {
            level = FactLocation.PLACE;
        }
        
        public CensusLocationComparer(int level) {
            this.level = level;
        }

        public override int Compare(CensusIndividual c1, CensusIndividual c2)
        {
            FactLocation l1 = c1.CensusLocation;
            FactLocation l2 = c2.CensusLocation;
            int comp = l1.CompareTo(l2, level);
            if (comp == 0) comp = base.Compare(c1, c2);
            return comp;
        }
    }
}