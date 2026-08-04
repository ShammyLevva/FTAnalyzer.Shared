namespace FTAnalyzer.Exports
{
    /// <summary>
    /// Builds the reference string Lost Cousins itself actually stores for a census entry, for use
    /// only when comparing against a My Ancestors page entry in LostCousinsReconciliation.
    /// CensusReference's own general-purpose Reference/CompactReference (used everywhere else in the
    /// app for display) doesn't match what Lost Cousins stores for every census year - e.g. a US
    /// 1880 citation's Enumeration District gets included in CompactReference but Lost Cousins' own
    /// USA1 field set for 1880 (see LostCousinsClient.GetCensusSpecificFields) has no ED field at
    /// all, and a "m-t0627-03764"-style 1940 roll keeps a leading zero that the website's own
    /// reference never has. Comparing against CompactReference directly makes these silently fail to
    /// match regardless of how well name/birth-year agree. Deliberately kept separate from
    /// CensusReference itself so quirks specific to Lost Cousins' matching don't affect the general
    /// reference display used throughout the rest of the app.
    /// </summary>
    public static class LostCousinsCensusReference
    {
        public static string Build(CensusReference? reference)
        {
            if (reference is null) return string.Empty;
            if (reference.Country == Countries.UNITED_STATES)
            {
                if (reference.CensusYear.Overlaps(CensusDate.USCENSUS1880))
                    return $"{reference.Roll.TrimStart('0')}/{reference.Page}";
                if (reference.CensusYear.Overlaps(CensusDate.USCENSUS1940))
                    return $"{reference.Roll.TrimStart('0')}/{reference.ED}/{reference.Page}";
            }
            return reference.CompactReference;
        }
    }
}
