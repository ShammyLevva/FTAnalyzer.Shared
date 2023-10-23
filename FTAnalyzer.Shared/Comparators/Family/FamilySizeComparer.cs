namespace FTAnalyzer
{
    public class FamilySizeComparer : Comparer<IDisplayFamily>
    {
        public bool CountSortLow { get; set; }

        public FamilySizeComparer(bool countSortLow) => CountSortLow = countSortLow;

        public override int Compare(IDisplayFamily? x, IDisplayFamily? y)
        {
            return CountSortLow
                ? x.FamilySize == y.FamilySize
                    ? string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal)
                    : x.FamilySize.CompareTo(y.FamilySize)
                : x.FamilySize == y.FamilySize
                    ? string.Compare(y.FamilyID, x.FamilyID, StringComparison.Ordinal)
                    : y.FamilySize.CompareTo(x.FamilySize);
        }
    }
}
