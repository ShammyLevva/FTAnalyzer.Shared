namespace FTAnalyzer
{
    public class AhnentafelComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual? x, IDisplayIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return x.Ahnentafel.CompareTo(y.Ahnentafel);
        }
    }
}
