namespace FTAnalyzer
{
    public class DefaultFamilyComparer : Comparer<IDisplayFamily>
    {

        public override int Compare(IDisplayFamily? x, IDisplayFamily? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return string.Compare(x.FamilyID, y.FamilyID, System.StringComparison.Ordinal);
        }
    }
}
