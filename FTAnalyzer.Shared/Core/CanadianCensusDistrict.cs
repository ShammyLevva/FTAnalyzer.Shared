using System.Reflection;
using System.Xml;

namespace FTAnalyzer
{
    /// <summary>
    /// Resolves an 1881 Canadian census District Number from a sub-district (township/parish) name,
    /// using Library and Archives Canada's own district/sub-district finding aid (bundled as
    /// Resources/CanadianCensusDistricts1881.xml - see the "Canadian Census Districts" source pages
    /// this was compiled from). The common Ancestry-style citation for this census only ever captures
    /// the microfilm Roll number (e.g. "Roll: C_13283") - a completely different identifier the
    /// District can't be derived from - but it does capture the "Census Place" text (township, county,
    /// province), which this class matches against LAC's sub-district names to recover the District.
    ///
    /// Deliberately name-based rather than Roll-based: LAC's own reel ranges cover several districts
    /// each (e.g. all of Manitoba's returns sit in just five reels), so a Roll number alone can't
    /// pinpoint a single District - the sub-district name is the far more precise key, and it's
    /// exactly what a Census Place citation already gives us.
    /// </summary>
    public static class CanadianCensusDistrict
    {
        // Sub-district names aren't unique nationally (or even always within a province), so two
        // lookup tiers: an exact (Province, Name) match when the citation's province is usable, and
        // a name-only fallback that only ever returns a result when that name is unambiguous across
        // the whole dataset. Never guess between multiple candidates - same conservative posture as
        // the rest of LostCousinsCensusReference.
        static readonly Dictionary<(string Province, string Name), string> BY_PROVINCE_AND_NAME = new();
        static readonly Dictionary<string, string> BY_NAME_IF_UNAMBIGUOUS = new(StringComparer.OrdinalIgnoreCase);
        static readonly HashSet<string> AMBIGUOUS_NAMES = new(StringComparer.OrdinalIgnoreCase);

        static readonly Lock LoadLock = new();
        static volatile bool _loaded;

        // Lazy, on-demand, lock-guarded load using the entry assembly's own directory - the same
        // pattern FactLocation.EnsureConversionsLoaded() uses. Deliberately NOT the ScottishParish
        // style "#if __PC__ static constructor" pattern: FTAnalyzer.Web defines __WEB__, not __PC__,
        // so that static constructor's body is empty there and LoadScottishParishes() never actually
        // runs under the web app - discovered while building this class, out of scope to fix here.
        static void EnsureLoaded()
        {
            if (_loaded) return;
            lock (LoadLock)
            {
                if (_loaded) return;
                LoadFromXml(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
                _loaded = true;
            }
        }

        public static void LoadFromXml(string? startPath)
        {
            if (startPath is null) return;
            string filename = Path.Combine(startPath, "Resources", "CanadianCensusDistricts1881.xml");
            if (!File.Exists(filename)) return;
            XmlDocument xmlDoc = new() { XmlResolver = null };
            string xml = File.ReadAllText(filename);
            using (XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings() { XmlResolver = null }))
                xmlDoc.Load(reader);
            if (xmlDoc.SelectNodes("CanadianCensusDistrict1881/ByName/SubDistrict") is not XmlNodeList nodeList) return;
            foreach (XmlNode n in nodeList)
            {
                XmlAttributeCollection? attrs = n.Attributes;
                if (attrs is null) continue;
                string province = attrs["Province"]?.Value ?? string.Empty;
                string districtNumber = attrs["DistrictNumber"]?.Value ?? string.Empty;
                string name = attrs["Name"]?.Value ?? string.Empty;
                if (name.Length == 0 || districtNumber.Length == 0) continue;
                AddEntry(province, name, districtNumber);
            }
        }

        // ValueTuple<string,string> equality is ordinal (case-sensitive), and a citation's casing/
        // spacing won't always match LAC's own text exactly - normalise both sides the same way so
        // lookups aren't case- or whitespace-sensitive.
        static string Normalise(string s) => s.Trim().ToUpperInvariant();

        static void AddEntry(string province, string name, string districtNumber)
        {
            string normName = Normalise(name);
            var key = (Normalise(province), normName);
            BY_PROVINCE_AND_NAME.TryAdd(key, districtNumber); // first wins on a genuine duplicate

            if (AMBIGUOUS_NAMES.Contains(normName)) return;
            if (BY_NAME_IF_UNAMBIGUOUS.TryGetValue(normName, out string? existing))
            {
                if (existing != districtNumber)
                {
                    BY_NAME_IF_UNAMBIGUOUS.Remove(normName);
                    AMBIGUOUS_NAMES.Add(normName);
                }
            }
            else
                BY_NAME_IF_UNAMBIGUOUS.Add(normName, districtNumber);
        }

        /// <summary>
        /// Returns the 1881 census District Number for a sub-district (township/parish) name, or
        /// string.Empty if it can't be resolved unambiguously. Tries an exact (province, name) match
        /// first, falling back to a name-only match only when that name is unique across the whole
        /// country - it never guesses between multiple same-named sub-districts.
        /// </summary>
        public static string FindDistrictNumber(string subDistrictName, string province)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(subDistrictName)) return string.Empty;
            string normName = Normalise(subDistrictName);
            if (!string.IsNullOrWhiteSpace(province) && BY_PROVINCE_AND_NAME.TryGetValue((Normalise(province), normName), out string? byProvince))
                return byProvince;
            return BY_NAME_IF_UNAMBIGUOUS.TryGetValue(normName, out string? byName) ? byName : string.Empty;
        }
    }
}
