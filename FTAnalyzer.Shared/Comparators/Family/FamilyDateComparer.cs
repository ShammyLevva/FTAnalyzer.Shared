namespace FTAnalyzer
{
    public class FamilyDateComparer : Comparer<Family>
    {
        public override int Compare(Family? x, Family? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return x.MarriageDate.CompareTo(y.MarriageDate);
        }
    }
}
