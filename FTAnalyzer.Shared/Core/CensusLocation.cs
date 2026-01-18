using System.Reflection;
using System.Xml;

namespace FTAnalyzer
{
    public class CensusLocation(string year, string piece, string rd, string parish, string county, string location)
    {
        static readonly Dictionary<Tuple<string, string>, CensusLocation> CENSUSLOCATIONS = [];
        public readonly static CensusLocation UNKNOWN = new(string.Empty);
        public readonly static CensusLocation SCOTLAND = new(Countries.SCOTLAND);
        public readonly static CensusLocation UNITED_STATES = new(Countries.UNITED_STATES);
        public readonly static CensusLocation CANADA = new(Countries.CANADA);
        public string Year { get; private set; } = year;
        public string Piece { get; private set; } = piece;
        public string RegistrationDistrict { get; private set; } = rd;
        public string Parish { get; private set; } = parish;
        public string County { get; private set; } = county;
        public string Location { get; private set; } = location;

#if __PC__
        static CensusLocation() => LoadCensusLocationFile(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
#elif __MACOS__
        static CensusLocation() => LoadCensusLocationFile(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), ".."));
#endif
        public static void LoadCensusLocationFile(string? startPath)
        {
            #region Census Locations
            // load Census Locations from XML file
            if (startPath is null) return;
            string filename = Path.Combine(startPath, "Resources", "CensusLocations.xml");
            if (File.Exists(filename))
            {
                XmlDocument xmlDoc = new() { XmlResolver = null };
                string xml = File.ReadAllText(filename);
                StringReader sreader = new(xml);
                using (XmlReader reader = XmlReader.Create(sreader, new XmlReaderSettings() { XmlResolver = null }))
                    xmlDoc.Load(reader);
                //xmlDoc.Validate(something);
                foreach (XmlNode n in xmlDoc.SelectNodes("CensusLocations/Location"))
                {
                    string year = n.Attributes["Year"].Value;
                    string piece = n.Attributes["Piece"].Value;
                    string RD = n.Attributes["RD"].Value;
                    string parish = n.Attributes["Parish"].Value;
                    string county = n.Attributes["County"].Value;
                    string location = n.InnerText;
                    CensusLocation cl = new(year, piece, RD, parish, county, location);
                    CENSUSLOCATIONS.Add(new Tuple<string, string>(year, piece), cl);
                }
            }
            #endregion
        }

        public CensusLocation(string location) : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, location) { }

        public static CensusLocation GetCensusLocation(string year, string piece)
        {
            if (piece == "MISSING") return UNKNOWN;
            Tuple<string, string> key = new(year, piece);
            CENSUSLOCATIONS.TryGetValue(key, out CensusLocation? result);
            return result ?? UNKNOWN;
        }
        public static CensusLocation Get1921CensusLocation(string regDistrict, string subDistrict)
        {
            if (regDistrict == "MISSING") return UNKNOWN;
            string piece = $"{regDistrict}/{subDistrict}";
            Tuple<string, string> key = new("1921", piece);
            CENSUSLOCATIONS.TryGetValue(key, out CensusLocation? result);
            return result ?? UNKNOWN;
        }

        public override string ToString() => Location.Length == 0 ? "UNKNOWN" : Location;
    }
}
