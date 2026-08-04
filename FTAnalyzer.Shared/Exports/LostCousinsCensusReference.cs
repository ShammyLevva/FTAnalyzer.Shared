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
            // BuildReference's Scotland branch returns "ParishName/RD/ED/Page" (via
            // ScottishParish.GetReference(true)) for general display, but Lost Cousins' own SCT1
            // schema (LostCousinsClient.GetCensusSpecificFields) only ever stores the bare
            // "RD/ED/Page" - no parish name field exists on the website at all, so the two can never
            // compare equal regardless of any trimming/casing.
            if (reference.Country == Countries.SCOTLAND && reference.CensusYear.Overlaps(CensusDate.SCOTCENSUS1881))
            {
                string rd = ScottishParish.FindParishFromID(reference.Parish).RegistrationDistrict;
                return $"{rd}/{reference.ED.TrimStart('0')}/{reference.Page.TrimStart('0')}";
            }
            // Lost Cousins' 0ENG schema for 1911 only has a Piece/Schedule pair (no Page field at
            // all). CensusReference.IsValidLostCousinsReference applies this same Piece prefix
            // stripping and the Schedule "9999" fallback (used when a citation only captured a page
            // number, not a schedule) - but only on the upload-validation path, never during normal
            // parsing, so BuildReference's un-trimmed Piece/Schedule (or its Piece/Page fallback,
            // which Lost Cousins has no matching field for) silently fails to match.
            if (Countries.IsEnglandWales(reference.Country) && reference.CensusYear.Overlaps(CensusDate.EWCENSUS1911))
            {
                string piece = reference.Piece;
                if (piece.StartsWith("RG14", StringComparison.Ordinal)) piece = piece[4..];
                else if (piece.StartsWith("PN", StringComparison.Ordinal)) piece = piece[2..];
                piece = piece.TrimStart('0');
                string schedule = reference.Schedule == CensusReference.MISSING ? string.Empty : reference.Schedule.TrimStart('0');
                if (schedule.Length == 0 && reference.Page.Length > 0) schedule = "9999";
                return $"{piece}/{schedule}";
            }
            // Lost Cousins' own reference for this census is "District/Page/Family" - three plain
            // numbers - but the common Ancestry citation for this census only ever captures the
            // microfilm Roll number (e.g. "Roll: C_13283"), a completely different identifier the
            // District can't be derived from. This only produces a matchable reference when a
            // citation captured the District explicitly (CensusReference's own "District NNN"
            // pattern, or one already written in this District/Page/Family format - see
            // Instructions#lc-reference-formats); otherwise it falls through unchanged below.
            if (reference.Country == Countries.CANADA && reference.CensusYear.Overlaps(CensusDate.CANADACENSUS1881)
                && reference.ED.Length > 0)
            {
                return $"{reference.ED.TrimStart('0')}/{reference.Page.TrimStart('0')}/{reference.Family.TrimStart('0')}";
            }
            return reference.CompactReference;
        }
    }
}
