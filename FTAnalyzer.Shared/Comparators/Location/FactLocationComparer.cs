namespace FTAnalyzer
{
    public class FactLocationComparer(int level) : Comparer<IDisplayLocation>
    {
        public int Level { get; } = level;

        public override int Compare(IDisplayLocation? x, IDisplayLocation? y)
        {
            if (x == null || y == null) return 0;
            return x.CompareTo(y, Level);
        }
    }
}
