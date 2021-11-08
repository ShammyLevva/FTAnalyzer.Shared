using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayLocation : IColumnComparer<IDisplayLocation>
    {
#if __PC__
        [ColumnDetail("Icon", 50, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.Icon)]
        System.Drawing.Image Icon { get; }
#endif
        [ColumnDetail("Country", 120)]
        string Country { get; }
        [ColumnDetail("Region", 120)]
        string Region { get; }
        [ColumnDetail("Sub Region", 140)]
        string SubRegion { get; }
        [ColumnDetail("Address", 160)]
        string Address { get; }
        [ColumnDetail("Place", 180)]
        string Place { get; }
        [ColumnDetail("Lat", 60)]
        double Latitude { get; }
        [ColumnDetail("Long", 60)]
        double Longitude { get; }
        [ColumnDetail("Geocode Status", 120)]
        string Geocoded { get; }
        [ColumnDetail("Found Location", 250)]
        string FoundLocation { get; }
        [ColumnDetail("GEDCOM Location", 250)]
        string GEDCOMLocation { get; }
        [ColumnDetail("GEDCOM Loaded", 80)]
        string GEDCOMLoaded { get; }

        int CompareTo(IDisplayLocation loc, int level);
        FactLocation GetLocation(int level);
    }
}
