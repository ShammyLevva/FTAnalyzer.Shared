using FTAnalyzer.Properties;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace FTAnalyzer
{
    public partial class ScottishParish
    {
        static readonly Dictionary<string, ScottishParish> SCOTTISHPARISHES = new();
        static readonly Dictionary<string, string> SCOTTISHPARISHNAMES = new();
        public readonly static ScottishParish UNKNOWN_PARISH = new("UNK", "Unknown", Countries.SCOTLAND);
        public string RegistrationDistrict { get; private set; }
        public FactLocation Location { get; private set; }
        public string Name { get; private set; }
        public string Region { get; private set; }

        static readonly Regex ParishRegex = RegexParish();
#if __PC__
        static ScottishParish() => LoadScottishParishes(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
#elif __MACOS__
        static ScottishParish() => LoadScottishParishes(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), ".."));
#endif
        public static void LoadScottishParishes(string? startPath)
        {
            // load Scottish Parishes from XML file
            if (startPath is null) return;
            string filename = Path.Combine(startPath, "Resources", "ScottishParishes.xml");
            if (File.Exists(filename))
            {
                XmlDocument xmlDoc = new() { XmlResolver = null };
                string xml = File.ReadAllText(filename);
                StringReader sreader = new(xml);
                using (XmlReader reader = XmlReader.Create(sreader, new XmlReaderSettings() { XmlResolver = null }))
                    xmlDoc.Load(reader);
                //xmlDoc.Validate(something);
                foreach (XmlNode n in xmlDoc.SelectNodes("ScottishParish/ByID/Parish"))
                {
                    string region = n.Attributes["Region"].Value;
                    string name = n.Attributes["Name"].Value;
                    string RD = n.Attributes["RD"].Value;
                    ScottishParish sp = new(RD, name, region);
                    AddParish(RD, sp);
                }
            }
        }

        static void AddParish(string RD, ScottishParish sp)
        {
            try
            {
                SCOTTISHPARISHES.Add(RD, sp);
                SCOTTISHPARISHNAMES.Add(sp.Name, RD);
            }
            catch (ArgumentException)
            { } // ignore duplicates leave first value in list
        }

        public ScottishParish(string RD, string name, string region)
        {
            RegistrationDistrict = RD;
            Name = name;
            Region = region;
            string loc = $"{name}, {region}, Scotland";
            Location = FactLocation.GetLocation(loc, false);
        }

        public static bool IsParishID(string rd)
        {
            if (rd is null) return false;
            if (int.TryParse(rd, out _))
                return true;
            rd = rd.ToLower().Replace(" ", "-").Replace("/", "-");
            Match match = ParishRegex.Match(rd);
            return match.Success;
        }

        public static ScottishParish FindParishFromID(string RD) => SCOTTISHPARISHES.ContainsKey(RD.ToLower()) ? SCOTTISHPARISHES[RD.ToLower()] : UNKNOWN_PARISH;
        public static string FindParishFromName(string parish) => SCOTTISHPARISHNAMES.TryGetValue(parish, out string? value) ? value : UNKNOWN_PARISH.Name;

        public string Reference => GeneralSettings.Default.UseCompactCensusRef ? $"{Name}/{RegistrationDistrict}" : $"{Name}, RD: {RegistrationDistrict}";

        public override string ToString() => $"RD: {RegistrationDistrict} Parish: {Name} Region: {Region}";
        [GeneratedRegex("\\d{1,3}-\\d{1,2}?[AB]?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
        private static partial Regex RegexParish();
    }
}
