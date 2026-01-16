namespace FTAnalyzer
{
    public class DisplayGreatStats(string relation, decimal sort, int count)
    {
        public string RelationToRoot { get; } = relation;
        public decimal RelationSort { get; } = sort;
        public int Count { get; } = count;
    }
}
