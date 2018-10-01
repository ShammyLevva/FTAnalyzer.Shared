using System.Collections.Generic;

namespace FTAnalyzer
{
    public class Region
    {
        public enum Creation { HISTORIC = 0, LG_ACT1974 = 1, MODERN = 2 }

        public string PreferredName { get; private set; }
        public string Country { get; private set; }
        public List<string> AlternativeNames { get; private set; }
        public string ISOcode { get; set; }
        public Creation RegionType { get; private set; }
        public List<ModernCounty> CountyCodes { get; private set; }
        
        public Region(string region, string country, Creation regionType)
        {
            Country = country;
            PreferredName = region;
            AlternativeNames = new List<string>();
            ISOcode = string.Empty;
            RegionType = regionType;
            CountyCodes = null;
        }

        public void AddAlternateName(string name) => AlternativeNames.Add(name);

        public void SetCountyCodes(List<ModernCounty> codes) => CountyCodes = codes;

        public override string ToString() => $"{PreferredName}, {Country}";
    }

    public class ModernCounty
    {
        public string CountyCode { get; private set; }
        public string CountyName { get; private set; }
        public string CountryName { get; private set; }

        public ModernCounty(string code, string countyName, string country)
        {
            CountyCode = code;
            CountyName = countyName;
            CountryName = country;
        }

        public override string ToString() => $"{CountyCode}: {CountyName}, {CountryName}";
    }
}
