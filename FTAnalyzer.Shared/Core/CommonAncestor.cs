namespace FTAnalyzer
{
    public class CommonAncestor
    {
        public Individual Ind { get; private set; }
        public int Distance { get; private set; }
        public bool Step { get; private set; }

        public CommonAncestor(Individual ind, int distance, bool step)
        {
            Ind = ind;
            Distance = distance;
            Step = step;
        }
    }
}
