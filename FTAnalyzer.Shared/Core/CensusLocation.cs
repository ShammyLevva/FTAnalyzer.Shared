using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace FTAnalyzer
{
    public class CensusLocation
    {
        static Dictionary<Tuple<string, string>, CensusLocation> CENSUSLOCATIONS = new Dictionary<Tuple<string, string>, CensusLocation>();
        public static CensusLocation UNKNOWN = new CensusLocation(string.Empty);
        public static CensusLocation SCOTLAND = new CensusLocation(Countries.SCOTLAND);
        public static CensusLocation UNITED_STATES = new CensusLocation(Countries.UNITED_STATES);
        public static CensusLocation CANADA = new CensusLocation(Countries.CANADA);
        public string Year { get; private set; }
        public string Piece { get; private set; }
        public string RegistrationDistrict { get; private set; }
        public string Parish { get; private set; }
        public string County { get; private set; }
        public string Location { get; private set; }

#if __PC__
        static CensusLocation() => LoadCensusLocationFile(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
#elif __MACOS__
        static CensusLocation() => LoadCensusLocationFile(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), ".."));
#endif
        public static void LoadCensusLocationFile(string startPath)
        {
#region Census Locations
            // load Census Locations from XML file
            if (startPath == null) return;
            string filename = Path.Combine(startPath, "Resources", "CensusLocations.xml");
            if (File.Exists(filename))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                //xmlDoc.Validate(something);
                foreach (XmlNode n in xmlDoc.SelectNodes("CensusLocations/Location"))
                {
                    string year = n.Attributes["Year"].Value;
                    string piece = n.Attributes["Piece"].Value;
                    string RD = n.Attributes["RD"].Value;
                    string parish = n.Attributes["Parish"].Value;
                    string county = n.Attributes["County"].Value;
                    string location = n.InnerText;
                    CensusLocation cl = new CensusLocation(year, piece, RD, parish, county, location);
                    CENSUSLOCATIONS.Add(new Tuple<string, string>(year, piece), cl);
                }
            }
#endregion
        }

        public CensusLocation(string location) : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, location) { }

        public CensusLocation(string year, string piece, string rd, string parish, string county, string location)
        {
            Year = year;
            Piece = piece;
            RegistrationDistrict = rd;
            Parish = parish;
            County = county;
            Location = location;
        }

        public static CensusLocation GetCensusLocation(string year, string piece)
        {
            Tuple<string, string> key = new Tuple<string, string>(year, piece);
            CENSUSLOCATIONS.TryGetValue(key, out CensusLocation result);
            return result ?? UNKNOWN;
        }

        public override string ToString() => Location.Length == 0 ? "UNKNOWN" : Location;
    }
}
