namespace FTAnalyzer
{
    public class DisplayGreatStats
    {
        public string RelationToRoot { get; }
        public long RelationSort { get; }
        public int Count { get; }

        public DisplayGreatStats(string relation, long sort, int count)
        {
            RelationToRoot = relation;
            RelationSort = sort;
            Count = count;
        }
    }
}
