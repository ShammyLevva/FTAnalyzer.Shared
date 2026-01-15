namespace FTAnalyzer
{
    public class DefaultIndividualComparer(bool ascending) : Comparer<Individual>
    {
        int Ascending { get; } = ascending ? 1 : -1;

        public override int Compare(Individual? x, Individual? y) =>
            Ascending * string.Compare(x.IndividualID, y.IndividualID, System.StringComparison.Ordinal);
    }
}
