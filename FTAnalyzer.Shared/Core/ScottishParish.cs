using FTAnalyzer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace FTAnalyzer
{
    public class ScottishParish
    {
        static Dictionary<string, ScottishParish> SCOTTISHPARISHES = new Dictionary<string, ScottishParish>();
        static Dictionary<string, string> SCOTTISHPARISHNAMES = new Dictionary<string, string>();
        public static ScottishParish UNKNOWN_PARISH = new ScottishParish("UNK", "Unknown", Countries.SCOTLAND);
        public string RegistrationDistrict { get; private set; }
        public FactLocation Location { get; private set; }
        public string Name { get; private set; }
        public string Region { get; private set; }

        static Regex ParishRegex = new Regex(@"\d{1,3}-\d{1,2}?[AB]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
#if __PC__
        static ScottishParish() => LoadScottishParishes(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
#elif __MACOS__
        static ScottishParish() => LoadScottishParishes(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), ".."));
#endif
        public static void LoadScottishParishes(string startPath)
        {
            // load Scottish Parishes from XML file
            if (startPath == null) return;
            string filename = Path.Combine(startPath, "Resources", "ScottishParishes.xml");
            if (File.Exists(filename))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                //xmlDoc.Validate(something);
                foreach (XmlNode n in xmlDoc.SelectNodes("ScottishParish/ByID/Parish"))
                {
                    string region = n.Attributes["Region"].Value;
                    string name = n.Attributes["Name"].Value;
                    string RD = n.Attributes["RD"].Value;
                    ScottishParish sp = new ScottishParish(RD, name, region);
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
            if (int.TryParse(rd, out int result))
                return true;
            rd.ToLower().Replace(" ", "-").Replace("/", "-");
            Match match = ParishRegex.Match(rd);
            return match.Success;
        }

        public static ScottishParish FindParishFromID(string RD) => SCOTTISHPARISHES.ContainsKey(RD.ToLower()) ? SCOTTISHPARISHES[RD.ToLower()] : UNKNOWN_PARISH;
        public static string FindParishFromName(string parish) => SCOTTISHPARISHNAMES.ContainsKey(parish) ? SCOTTISHPARISHNAMES[parish] : UNKNOWN_PARISH.Name;

        public string Reference => GeneralSettings.Default.UseCompactCensusRef ? $"{Name}/{RegistrationDistrict}" : $"{Name}, RD: {RegistrationDistrict}";

        public override string ToString() => $"RD: {RegistrationDistrict} Parish: {Name} Region: {Region}";
    }
}
