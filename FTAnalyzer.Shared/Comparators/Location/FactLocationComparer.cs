namespace FTAnalyzer
{
    public class FactLocationComparer(int level) : Comparer<IDisplayLocation>
    {
        public int Level { get; } = level;

        public override int Compare(IDisplayLocation? x, IDisplayLocation? y)
        {
            if (x is null && y is null)
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;
            return x.CompareTo(y, Level);
        }
    }
}
