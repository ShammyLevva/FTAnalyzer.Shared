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
            if (Countries.IsEnglandWales(reference.Country) && reference.CensusYear.Overlaps(CensusDate.EWCENSUS1841))
            {
                // Lost Cousins' 1841 schema (census_code=1841, see GetCensusSpecificFields) is
                // Piece/Book/Folio/Page - and the live "Add an Ancestor" form marks all four as
                // required, so a citation with no Book number can never match regardless of how this
                // string is built (Lost Cousins' own stored reference will always have a real Book
                // value, and ours has none). What this fixes is narrower: BuildReference's compact
                // display form substitutes literal "see image" text when Book is blank (a hint for
                // the user - see Instructions#lc-reference-formats - not a stored value), and that
                // text must never leak into a comparison string even though the comparison was
                // already doomed to fail - the old behaviour was merely wrong for the wrong reason.
                // Build the plain field list directly, dropping the MISSING sentinel the same way the
                // 1911 branch below already does for Schedule.
                return JoinFields(reference.Piece, NonMissing(reference.Book), NonMissing(reference.Folio), reference.Page);
            }
            if (Countries.IsEnglandWales(reference.Country) && reference.CensusYear.Overlaps(CensusDate.EWCENSUS1881))
            {
                // Lost Cousins' 1881 schema (census_code=RG11) is Piece/Folio/Page only - no
                // Schedule field exists on the website at all - but BuildReference's compact form
                // falls back to a "Piece/Folio/SN{Schedule}" form when Page wasn't captured, which
                // has no equivalent to match against. Build the plain field list directly instead.
                return JoinFields(reference.Piece, reference.Folio, reference.Page);
            }
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
            // numbers. Despite GetCensusSpecificFields sending a Sub-District as a separate ref2
            // field on upload, live testing against a real Lost Cousins account (see
            // CensusReferenceCanadaTests in UnitTests/CensusReferenceTest.cs) confirmed the website's
            // own stored/scraped reference never has a separate Sub-District segment - it's either
            // ignored server-side or folded away - so Sub-District must NOT be included here despite
            // what the upload schema implies. The common Ancestry citation for this census only ever
            // captures the microfilm Roll number (e.g. "Roll: C_13283") instead, a completely
            // different identifier the District can't be derived from (LAC's own reel ranges cover
            // several districts each). What that citation DOES capture is the "Census Place" text -
            // e.g. "Woodlands, Marquette, Manitoba" - and CanadianCensusDistrict resolves the
            // township/sub-district name (the first segment) against LAC's own district finding aid
            // to recover the District, using the province (the last segment) to disambiguate
            // same-named sub-districts. Falls through unchanged if neither the District nor a
            // resolvable place name is available (e.g. a citation already written in
            // District/Page/Family format - see Instructions#lc-reference-formats - still works via
            // the ED it was parsed into).
            if (reference.Country == Countries.CANADA && reference.CensusYear.Overlaps(CensusDate.CANADACENSUS1881))
            {
                string ed = reference.ED;
                if (ed.Length == 0 && reference.Place.Length > 0)
                {
                    string[] parts = reference.Place.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        ed = CanadianCensusDistrict.FindDistrictNumber(parts[0], parts[^1]);
                }
                if (ed.Length > 0)
                    return JoinFields(ed.TrimStart('0'), reference.Page.TrimStart('0'), reference.Family.TrimStart('0'));
            }
            return reference.CompactReference;
        }

        // For an unmatched Lost Cousins entry, the exact note text that (if added to that person's
        // source citation) CensusReference's own parsing will recognise and rebuild into this same
        // reference - a documented last-resort format already used for Canada 1881 (see
        // Instructions#lc-reference-formats). Round-trip correctness (this exact output, fed back in
        // as a citation, parses to GOOD with the same field values) is checked directly against
        // CensusReference's real parsing path in LostCousinsCensusReferenceSuggestionTest - anyone
        // changing either side must keep both in sync.
        //
        // Lost Cousins itself supports nine censuses (see the "Add an Ancestor" form's own drop-
        // down); this method offers a quick-fix note for six of them, using whichever existing
        // CensusReference pattern accepts a bare number-only reference in the same field order Lost
        // Cousins' own website reference already comes in (see Utilities/RegexPatterns.cs) - the LC-
        // specific LC_CENSUS_PATTERN_* ones for the four that need a trailing "<census> <year>" hint
        // to disambiguate, plus one general-purpose pattern that doesn't need one at all:
        //  - Scotland 1881 uses LC_CENSUS_PATTERN_SCOT's Registration District number directly as
        //    the Parish field - which reads oddly given the field name, but matches how
        //    ScottishParish.FindParishFromID already works: it's keyed by RD, not by a separate
        //    parish name/ID, and ScottishParishes.xml gives every RD its own single parish (verified:
        //    zero duplicate RD values across all 1197 entries) - so RD round-trips through it
        //    exactly, with no ambiguity to resolve.
        //  - England & Wales 1841 uses EW_CENSUS_1841_51_PATTERN8 ("HO107/Piece/Book/Folio/Page"),
        //    a bare slash-separated form that (unlike the LC-specific pattern for 1881/1911, which
        //    only has room for 3 numbers) already captures all 4 fields 1841 needs, in the same
        //    order Lost Cousins' own reference has them.
        // The other three fall through to the explanatory message below, for two different reasons:
        //  - US 1880 has no such format defined at all - every existing US census pattern requires
        //    an Enumeration District field, which Lost Cousins' own 1880 reference never has (it's
        //    Roll/Page only).
        //  - Ireland 1911 and Newfoundland 1921 aren't matched by Family Tree Analyzer at all yet
        //    (see Instructions#lc-reference-formats) - there's no CensusReference pattern for either,
        //    so no note could ever help regardless of format. Newfoundland has no CensusDate
        //    constant at all (website.CensusDate is null for it), which the fallthrough below
        //    handles the same way as any other unrecognised value.
        //
        // websiteReference is stripped of every whitespace character first, not just collapsed -
        // every pattern below requires the digits and slashes to run together with nothing in
        // between, and nothing guarantees the website's own page rendering never puts a stray space
        // around a separator (ClearWhiteSpace, applied when this value was scraped, only collapses
        // repeated whitespace, it doesn't remove single spaces).
        public static string? SuggestedCitationNote(string websiteReference, CensusDate? censusDate)
        {
            if (string.IsNullOrWhiteSpace(websiteReference)) return null;
            string reference = new string(websiteReference.Where(c => !char.IsWhiteSpace(c)).ToArray());
            // England & Wales 1841 needs a literal "HO107/" prefix instead of a trailing "<census>
            // <year>" hint - EW_CENSUS_1841_51_PATTERN8 doesn't look for one at all, and there's no
            // 3-number LC-specific pattern to disambiguate from the way the other years need.
            if (ReferenceEquals(censusDate, CensusDate.EWCENSUS1841))
                return $"HO107/{reference}";
            // Reference equality, not CensusDate.Equals (inherited from FactDate, which compares only
            // the calendar date): two unrelated censuses can fall on the same calendar date - England
            // & Wales 1881 and Scotland 1881 were both taken the night of "03 APR 1881", and England
            // & Wales 1911 and Ireland 1911 both on "02 APR 1911" - so EWCENSUS1881.Equals(
            // SCOTCENSUS1881) and EWCENSUS1911.Equals(IRELANDCENSUS1911) are both true even though
            // each pair has an incompatible reference format. Value equality would have this method
            // hand back an England & Wales-formatted note for a Scottish or Irish entry. Every
            // CensusDate relevant here (including website.CensusDate, which only ever gets assigned
            // one of these exact static fields - see LostCousin's constructors) is a fixed singleton,
            // so comparing by reference is both safe and exact.
            string? suffix =
                ReferenceEquals(censusDate, CensusDate.EWCENSUS1881) ? "England & Wales 1881" :
                ReferenceEquals(censusDate, CensusDate.EWCENSUS1911) ? "England & Wales 1911" :
                ReferenceEquals(censusDate, CensusDate.CANADACENSUS1881) ? "Canada 1881" :
                ReferenceEquals(censusDate, CensusDate.USCENSUS1940) ? "US 1940" :
                ReferenceEquals(censusDate, CensusDate.SCOTCENSUS1881) ? "Scotland 1881" :
                null;
            return suffix is not null
                ? $"{reference} {suffix}"
                : "No quick-fix note for this census - see the reference format guide";
        }

        // Lost Cousins collapses a blank field to nothing rather than an empty slot (see
        // LostCousin.FixReference, which replaces "&refN=" markers with "/" then collapses the
        // resulting "//" - i.e. exactly this filter-then-join), so building a reference to compare
        // against a scraped website entry must drop blanks the same way rather than embedding an
        // empty segment (or, worse, a display-only placeholder - see NonMissing below).
        static string JoinFields(params string[] fields) => string.Join('/', fields.Where(f => f.Length > 0));

        // CensusReference sets a field to the literal sentinel "Missing" (CensusReference.MISSING)
        // when a citation was recognised but a particular field couldn't be parsed out of it at all -
        // that sentinel is meant for display (e.g. BuildReference's compact form), never as a value
        // to compare against Lost Cousins' own blank field, so treat it as blank here too.
        static string NonMissing(string field) => field == CensusReference.MISSING ? string.Empty : field;
    }
}
