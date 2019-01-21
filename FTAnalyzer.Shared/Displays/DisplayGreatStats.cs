namespace FTAnalyzer
{
    public class DisplayGreatStats
    {
        public string RelationToRoot { get; }
        public decimal RelationSort { get; }
        public int Count { get; }

        public DisplayGreatStats(string relation, decimal sort, int count)
        {
            RelationToRoot = relation;
            RelationSort = sort;
            Count = count;
        }
    }
}
