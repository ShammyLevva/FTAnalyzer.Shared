using System.Collections.Generic;

namespace FTAnalyzer
{
    public class IndividualBudgieComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            // change the + for older to an Z and - for younger to a A to force sort to be right
            string x1 = x.BudgieCode.Length == 0 ? "X" : x.BudgieCode.Replace('+', 'z').Replace('-', 'a');
            string y1 = y.BudgieCode.Length == 0 ? "X" : y.BudgieCode.Replace('+', 'z').Replace('-', 'a');
            return string.Compare(x1, y1, System.StringComparison.Ordinal);
        }
    }
}
