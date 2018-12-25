using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DisplayOccupation : IDisplayOccupation
    {
        public string Occupation { get; }
        public int Count { get; }

        public DisplayOccupation(string occupation,int count)
        {
            Occupation = occupation;
            Count = count;
        }

        public int CompareTo(IDisplayOccupation that) => string.Compare(Occupation, that.Occupation, System.StringComparison.Ordinal);

        public IComparer<IDisplayOccupation> GetComparer(string columnName, bool ascending)
        {
            throw new System.NotImplementedException();
        }
    }
}
