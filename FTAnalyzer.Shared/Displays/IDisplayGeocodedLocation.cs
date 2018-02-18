#if !__MACOS__
using FTAnalyzer.Mapping;
#endif

namespace FTAnalyzer
{
    public interface IDisplayGeocodedLocation
    {
        string SortableLocation { get; }
        double Latitude { get; }
        double Longitude { get; }
#if !__MACOS__
        System.Drawing.Image Icon { get; }
#endif
        string Geocoded { get; }
        string FoundLocation { get; }
        string FoundResultType { get; }
#if !__MACOS__
        GeoResponse.CResult.CGeometry.CViewPort ViewPort { get; }
#endif
    }
}
