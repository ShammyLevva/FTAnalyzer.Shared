using System.Collections.Generic;

namespace FTAnalyzer
{
    public class ExportFactsAtLocation
    {
        public string SortableLocation { get; set; }
        public string LocationName { get; set; }
        public List<string> FactsAtLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class FactsAtLocationComparer : Comparer<ExportFactsAtLocation>
    {
        public FactsAtLocationComparer() { }

        public override int Compare(ExportFactsAtLocation? x, ExportFactsAtLocation? y)
        {
            return x.SortableLocation.CompareTo(y.SortableLocation);
        }
    }
}
