namespace FTAnalyzer
{
    public class Region(string region, string country, Region.Creation regionType)
    {
        public enum Creation { HISTORIC = 0, LG_ACT1974 = 1, MODERN = 2 }

        public string PreferredName { get; private set; } = region;
        public string Country { get; private set; } = country;
        public List<string> AlternativeNames { get; private set; } = [];
        public string ISOcode { get; set; } = string.Empty;
        public Creation RegionType { get; private set; } = regionType;
        public List<ModernCounty>? CountyCodes { get; private set; }

        public void AddAlternateName(string name) => AlternativeNames.Add(name);

        public void SetCountyCodes(List<ModernCounty> codes) => CountyCodes = codes;

        public override string ToString() => $"{PreferredName}, {Country}";
    }

    public class ModernCounty(string code, string countyName, string country)
    {
        public string CountyCode { get; private set; } = code;
        public string CountyName { get; private set; } = countyName;
        public string CountryName { get; private set; } = country;

        public override string ToString() => $"{CountyCode}: {CountyName}, {CountryName}";
    }
}
