using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayLocation
    {
        [ColumnWidth(120)]
        string Country { get; }
        [ColumnWidth(120)]
        string Region { get; }
        [ColumnWidth(140)]
        string SubRegion { get; }
        [ColumnWidth(160)]
        string Address { get; }
        [ColumnWidth(180)]
        string Place { get; }
        [ColumnWidth(60)]
        double Latitude { get; }
        [ColumnWidth(60)]
        double Longitude { get; }
#if __PC__
        System.Drawing.Image Icon { get; }
#endif
        [ColumnWidth(120)]
        string Geocoded { get; }
        [ColumnWidth(250)]
        string FoundLocation { get; }

        int CompareTo(IDisplayLocation loc, int level);
        FactLocation GetLocation(int level);

    }
}
