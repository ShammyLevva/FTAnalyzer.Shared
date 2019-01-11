using FTAnalyzer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace FTAnalyzer
{
    public class ScottishParish
    {
        static Dictionary<string, ScottishParish> SCOTTISHPARISHES = new Dictionary<string, ScottishParish>();
        public static ScottishParish UNKNOWN_PARISH = new ScottishParish("UNK", "", Countries.SCOTLAND);
        public string RegistrationDistrict { get; private set; }
        public FactLocation Location { get; private set; }
        public string Name { get; private set; }
        public string Region { get; private set; }

        static ScottishParish()
        {
            LoadScottishParishes(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
        }

        public static void LoadScottishParishes(string startPath)
        {
            // load Scottish Parishes from XML file
            if (startPath == null) return;
            string filename = Path.Combine(startPath, @"Resources\ScottishParishes.xml");
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

        public static ScottishParish FindParish(string RD) => SCOTTISHPARISHES.ContainsKey(RD) ? SCOTTISHPARISHES[RD] : UNKNOWN_PARISH;

        public string Reference
        {
            get
            {
                if (GeneralSettings.Default.UseCompactCensusRef)
                    return $"{Name}/{RegistrationDistrict}";
                return $"{Name}, RD: {RegistrationDistrict}";
            }
        }

        public override string ToString() => $"RD: {RegistrationDistrict} Parish: {Name} Region: {Region}";
    }
}
