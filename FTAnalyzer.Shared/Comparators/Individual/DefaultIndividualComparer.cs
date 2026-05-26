namespace FTAnalyzer
{
    public class DefaultIndividualComparer(bool ascending) : Comparer<Individual>
    {
        int Ascending { get; } = ascending ? 1 : -1;

        public override int Compare(Individual? x, Individual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return Ascending * -1;
            if (y is null) return Ascending * 1;
            return Ascending * string.Compare(x.IndividualID, y.IndividualID, StringComparison.Ordinal);
        }
    }
}
