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

        static readonly Lock LoadLock = new();
        static volatile bool _loaded;

        // Lazy, on-demand, lock-guarded load using the entry assembly's own directory - same
        // pattern as CanadianCensusDistrict.EnsureLoaded()/ScottishParish.EnsureLoaded(). Deliberately
        // NOT a "#if __PC__ static constructor" (the previous approach): FTAnalyzer.Web defines
        // __WEB__, not __PC__, so that static constructor's body was empty there and
        // LoadCensusLocationFile() never actually ran under the web app.
        static void EnsureLoaded()
        {
            if (_loaded) return;
            lock (LoadLock)
            {
                if (_loaded) return;
                LoadCensusLocationFile(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
                _loaded = true;
            }
        }

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
                if (xmlDoc.SelectNodes("CensusLocations/Location") is not XmlNodeList nodeList) return;
                foreach (XmlNode n in nodeList)
                {
                    if (n.Attributes is null) continue;
                    string year = n.Attributes["Year"]?.Value ?? string.Empty;
                    string piece = n.Attributes["Piece"]?.Value ?? string.Empty;
                    string RD = n.Attributes["RD"]?.Value ?? string.Empty;
                    string parish = n.Attributes["Parish"]?.Value ?? string.Empty;
                    string county = n.Attributes["County"]?.Value ?? string.Empty;
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
            if (piece == "Missing") return UNKNOWN;
            EnsureLoaded();
            Tuple<string, string> key = new(year, piece);
            CENSUSLOCATIONS.TryGetValue(key, out CensusLocation? result);
            return result ?? UNKNOWN;
        }
        public static CensusLocation Get1921CensusLocation(string regDistrict, string subDistrict)
        {
            if (regDistrict == "Missing") return UNKNOWN;
            EnsureLoaded();
            string piece = $"{regDistrict}/{subDistrict}";
            Tuple<string, string> key = new("1921", piece);
            CENSUSLOCATIONS.TryGetValue(key, out CensusLocation? result);
            return result ?? UNKNOWN;
        }

        public override string ToString() => Location.Length == 0 ? "UNKNOWN" : Location;
    }
}
