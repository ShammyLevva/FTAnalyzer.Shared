namespace FTAnalyzer
{
    public class IndividualBudgieComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual? x, IDisplayIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            // change the + for older to an Z and - for younger to a A to force sort to be right
            string x1 = x.BudgieCode.Length == 0 ? "X" : x.BudgieCode.Replace('+', 'z').Replace('-', 'a');
            string y1 = y.BudgieCode.Length == 0 ? "X" : y.BudgieCode.Replace('+', 'z').Replace('-', 'a');
            return string.Compare(x1, y1, StringComparison.Ordinal);
        }
    }
}
