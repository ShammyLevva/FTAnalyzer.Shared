using System.Security;
using System.Text;

namespace FTAnalyzer
{
    public static class KmlExporter
    {
        public const long KmzThresholdBytes = 5_000_000;
        const int MaxDescriptionLines = 100;

        public static string GenerateKml(IEnumerable<ExportFactsAtLocation> locations)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"">");
            sb.AppendLine("<Document>");

            foreach (ExportFactsAtLocation loc in locations)
            {
                if (loc.FactsAtLocation.Count == 0)
                    continue;

                sb.AppendLine("<Placemark>");
                sb.AppendLine($"    <name>{SecurityElement.Escape(loc.LocationName)}</name>");
                sb.AppendLine("    <description>The following individuals/families were here:");
                for (int i = 0; i < loc.FactsAtLocation.Count && i < MaxDescriptionLines; i++)
                    sb.AppendLine(SecurityElement.Escape(loc.FactsAtLocation[i]));
                if (loc.FactsAtLocation.Count > MaxDescriptionLines)
                {
                    int remaining = loc.FactsAtLocation.Count - MaxDescriptionLines;
                    sb.AppendLine($"and {remaining} more. (Google limit max {MaxDescriptionLines} lines).");
                }
                sb.AppendLine("    </description>");
                sb.AppendLine("    <Point>");
                sb.AppendLine($"        <coordinates>{loc.Longitude},{loc.Latitude},0</coordinates>");
                sb.AppendLine("    </Point>");
                sb.AppendLine("</Placemark>");
            }

            sb.AppendLine("</Document>");
            sb.AppendLine("</kml>");
            return sb.ToString();
        }
    }
}
