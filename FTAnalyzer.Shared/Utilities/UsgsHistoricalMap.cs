namespace FTAnalyzer.Shared.Utilities
{
    // Free, keyless Esri Living Atlas ImageServer backing the USGS historical topographic map
    // layer offered on both the desktop (BruTile/SharpMap) and web (OpenLayers) mapping UIs -
    // centralised here so the two independent renderers can't drift on the URL or year range.
    public static class UsgsHistoricalMap
    {
        public const string ImageServerUrl =
            "https://utility.arcgis.com/usrsvcs/servers/88d12190e2494ce89374311800af4c4a/rest/services/USGS_Historical_Topographic_Maps/ImageServer";

        public const string AttributionUrl = "https://livingatlas.arcgis.com/topomapexplorer/";

        public const int MinYear = 1884;
        public const int MaxYear = 2006;
        public const int DefaultYear = 1950;
    }
}
