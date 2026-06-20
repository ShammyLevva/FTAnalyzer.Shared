namespace FTAnalyzer
{
    public class ExportFactsAtLocation
    {
        public string SortableLocation { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public List<string> FactsAtLocation { get; set; } = [];
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class FactsAtLocationComparer : Comparer<ExportFactsAtLocation>
    {
        public FactsAtLocationComparer() { }

        public override int Compare(ExportFactsAtLocation? x, ExportFactsAtLocation? y)
        {
            return string.Compare(x?.SortableLocation, y?.SortableLocation, StringComparison.Ordinal);
        }
    }
}
