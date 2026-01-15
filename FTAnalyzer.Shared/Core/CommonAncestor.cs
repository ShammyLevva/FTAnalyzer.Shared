namespace FTAnalyzer
{
    public class CommonAncestor(Individual ind, int distance, bool step)
    {
        public Individual Ind { get; private set; } = ind;
        public int Distance { get; private set; } = distance;
        public bool Step { get; private set; } = step;
    }
}
