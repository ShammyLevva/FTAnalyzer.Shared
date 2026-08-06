namespace FTAnalyzer.Exports
{
    /// <summary>
    /// Reconciles the entries scraped from a user's Lost Cousins My Ancestors page against the
    /// tree's own list of upload candidates (individuals with a recognised census reference but
    /// no Lost Cousins fact recorded locally). This is the "verify" step that never got built:
    /// census reference is the reliable key (it identifies the household/family group on the
    /// census return), with forename matching only needed to disambiguate multiple household
    /// members sharing the same reference. Plain name equality and DoubleMetaphone both fail on
    /// nickname pairs (Margaret/Maggie are different words, not phonetic variants), so names are
    /// also compared via FamilyTree.Instance.GetStandardisedName, which maps name variants to a
    /// canonical form using the researched GINAP name-standardisation dataset (Resources/GINAP.txt).
    /// A reference isn't always unique to one household, though - EW1841's Piece/Book/Folio/Page
    /// identifies a whole census PAGE, which can hold more than one family (a second dwelling, a
    /// lodger recorded under a different surname) - so surname is also checked, both to pick the
    /// right website entry for a candidate and to resolve two different candidates that both
    /// independently matched the SAME website entry (see Reconcile's dedup step).
    /// </summary>
    public static class LostCousinsReconciliation
    {
        public readonly record struct Match(CensusIndividual Individual, LostCousin WebsiteEntry);

        // Per-run memoization for the per-candidate values NamesMatch/SurnamesMatch need -
        // SurnameAtDate walks a woman's marriage history (a LINQ sort) on every call, and the
        // metaphone keys involve constructing a new DoubleMetaphone, so recomputing either of these
        // for the same candidate on every single comparison (this class does an O(candidates ×
        // website entries) style comparison, so the same candidate is compared against many website
        // entries) is wasteful - with several thousand records this was measurably slow. Scoped to
        // one Reconcile/FindPossibleMatches call, not shared across calls, so there's no staleness
        // risk if the underlying tree changes between calls.
        sealed class CandidateNameCache
        {
            readonly Dictionary<CensusIndividual, string> _surnameAtDate = [];
            readonly Dictionary<CensusIndividual, string> _forenameMetaphone = [];
            readonly Dictionary<CensusIndividual, string> _surnameMetaphone = [];

            public string SurnameAtDate(CensusIndividual c)
            {
                if (!_surnameAtDate.TryGetValue(c, out string? surname))
                    _surnameAtDate[c] = surname = c.SurnameAtDate(c.CensusDate);
                return surname;
            }

            public string ForenameMetaphone(CensusIndividual c)
            {
                if (!_forenameMetaphone.TryGetValue(c, out string? key))
                    _forenameMetaphone[c] = key = new DoubleMetaphone(c.LCForename).PrimaryKey;
                return key;
            }

            public string SurnameMetaphone(CensusIndividual c)
            {
                if (!_surnameMetaphone.TryGetValue(c, out string? key))
                    _surnameMetaphone[c] = key = new DoubleMetaphone(SurnameAtDate(c)).PrimaryKey;
                return key;
            }
        }

        public static (List<CensusIndividual> StillMissing, List<Match> ConfirmedOnWebsite) Reconcile(
            IReadOnlyList<LostCousin> websiteAncestors, IReadOnlyList<CensusIndividual> candidates)
        {
            List<CensusIndividual> stillMissing = [];
            List<Match> allMatches = [];
            CandidateNameCache cache = new();

            Dictionary<string, List<LostCousin>> websiteByReference = websiteAncestors
                .Where(w => !string.IsNullOrWhiteSpace(w.Reference))
                .GroupBy(w => Normalise(w.Reference))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (CensusIndividual candidate in candidates)
            {
                LostCousin? match = FindMatch(candidate, websiteByReference, cache);
                if (match is not null)
                    allMatches.Add(new Match(candidate, match));
                else
                    stillMissing.Add(candidate);
            }

            // Two different candidates can independently satisfy FindMatch for the SAME website
            // entry - a shared reference spanning more than one household (see class doc comment)
            // means candidate A's own FindMatch call might grab candidate B's rightful entry (or
            // vice versa) via the surname-blind fallback tier inside FindMatch. Rather than let an
            // arbitrary one win, prefer whichever candidate's surname actually agrees with the
            // website entry's; the loser goes back into stillMissing instead of keeping a wrong
            // "confirmed" match - which, via CreateConfirmationFact, would otherwise write a
            // fabricated Lost Cousins fact onto the wrong person.
            List<Match> confirmed = [.. allMatches
                .GroupBy(m => m.WebsiteEntry)
                .Select(g => g.OrderByDescending(m => SurnamesMatch(m.WebsiteEntry, m.Individual, cache)).First())];

            HashSet<CensusIndividual> confirmedIndividuals = [.. confirmed.Select(m => m.Individual)];
            stillMissing.AddRange(allMatches.Select(m => m.Individual).Where(i => !confirmedIndividuals.Contains(i)));

            return (stillMissing, confirmed);
        }

        static LostCousin? FindMatch(CensusIndividual candidate, Dictionary<string, List<LostCousin>> websiteByReference, CandidateNameCache cache)
        {
            // Lost Cousins' own website always shows references in compact slash form, regardless of
            // this user's "Use compact census references" display preference - compare like for like.
            // Built via LostCousinsCensusReference, not candidate.CompactCensusRef directly, since
            // Lost Cousins' own reference doesn't always contain the same fields CensusReference's
            // general-purpose display string does (see LostCousinsCensusReference for why). Built from
            // HouseholdCensusReference, not CensusReference - Lost Cousins pins every household member
            // to the head of household's own reference, even for someone whose own citation correctly
            // captured a different (typically later) census page the household overflowed onto.
            string candidateRef = LostCousinsCensusReference.Build(candidate.HouseholdCensusReference);
            if (string.IsNullOrWhiteSpace(candidateRef))
                return null;
            if (!websiteByReference.TryGetValue(Normalise(candidateRef), out List<LostCousin>? group))
                return null;

            // FactDate/CensusDate's GetHashCode is reference-based while == is value-based, so
            // compare with the operator directly rather than trusting it as a dictionary key.
            List<LostCousin> sameCensus = [.. group.Where(w => w.CensusDate is null || w.CensusDate == candidate.CensusDate)];
            if (sameCensus.Count == 1)
                return sameCensus[0];

            // Prefer a website entry whose surname actually agrees over the surname-blind fallback -
            // a shared reference can span more than one household (see class doc comment), so
            // forename+birth-year alone isn't always enough to avoid grabbing someone else's entry.
            return sameCensus.FirstOrDefault(w => NamesMatch(w, candidate, cache) && SurnamesMatch(w, candidate, cache) && BirthYearsAgree(w, candidate))
                ?? sameCensus.FirstOrDefault(w => NamesMatch(w, candidate, cache) && BirthYearsAgree(w, candidate));
        }

        static bool BirthYearsAgree(LostCousin website, CensusIndividual candidate) =>
            website.BirthYear <= 0 || !candidate.BirthDate.IsKnown || Math.Abs(website.BirthYear - candidate.BirthDate.StartDate.Year) <= 5;

        static bool NamesMatch(LostCousin website, CensusIndividual candidate, CandidateNameCache cache)
        {
            string webForename = ExtractForename(website.Name);
            if (string.IsNullOrEmpty(webForename) || string.IsNullOrEmpty(candidate.LCForename))
                return false;
            if (string.Equals(webForename, candidate.LCForename, StringComparison.OrdinalIgnoreCase))
                return true;
            // GetStandardisedName maps name variants (Maggie, Margaret, Peggy, Madge, ...) to the
            // same canonical form via the GINAP dataset, so this catches nicknames metaphone can't.
            string webStandardised = FamilyTree.Instance.GetStandardisedName(candidate.IsMale, webForename);
            if (string.Equals(webStandardised, candidate.StandardisedName, StringComparison.OrdinalIgnoreCase))
                return true;
            // website.ForenameMetaphone was already computed once in LostCousin.SetMetaphones - only
            // the candidate side needs computing (and caching) here.
            return website.ForenameMetaphone == cache.ForenameMetaphone(candidate);
        }

        // Used both by FindMatch/Reconcile above (a shared reference can span more than one
        // household, so forename+birth-year alone isn't always a safe disambiguator - see class doc
        // comment) and by FindPossibleMatches below (which has no reference to narrow by at all, so
        // without this it would happily match any same-forename, same-birth-year person anywhere in
        // the tree). Compares against the candidate's OWN SurnameAtDate (via cache) rather than
        // CensusSurname (the family's surname) - correctly walks a woman's marriage history up to the
        // census date herself, rather than depending on the CensusFamily grouping (Husband/Wife/
        // eldest child) she happens to have been placed in already being right.
        static bool SurnamesMatch(LostCousin website, CensusIndividual candidate, CandidateNameCache cache)
        {
            string webSurname = ExtractSurname(website.Name);
            string candidateSurname = cache.SurnameAtDate(candidate);
            if (string.IsNullOrEmpty(webSurname) || string.IsNullOrEmpty(candidateSurname))
                return false;
            if (string.Equals(webSurname, candidateSurname, StringComparison.OrdinalIgnoreCase))
                return true;
            // website.SurnameMetaphone was already computed once in LostCousin.SetMetaphones.
            return website.SurnameMetaphone == cache.SurnameMetaphone(candidate);
        }

        // LostCousin.Name is "Surname, Forename(s)" (see LostCousin.SetMetaphones).
        static string ExtractForename(string name)
        {
            string forenames = ExtractForenames(name);
            int space = forenames.IndexOf(' ');
            return space > 0 ? forenames[..space] : forenames;
        }

        // Full forename(s) - e.g. "Andrew Skene Kelman" - as opposed to ExtractForename's first-word-
        // only. Only used by FindPossibleMatches, to tell two people sharing a common first forename
        // (e.g. two different "Andrew Bisset"s) apart when there's no census reference left to
        // disambiguate them by.
        static string ExtractForenames(string name)
        {
            int comma = name.IndexOf(',');
            return comma < 0 || comma + 2 > name.Length ? name : name[(comma + 2)..];
        }

        static string ExtractSurname(string name)
        {
            int comma = name.IndexOf(',');
            return comma < 0 ? string.Empty : name[..comma];
        }

        static bool FullNameMatches(LostCousin website, CensusIndividual candidate) =>
            string.Equals(ExtractForenames(website.Name), candidate.Forenames, StringComparison.OrdinalIgnoreCase);

        static bool ExactBirthYear(LostCousin website, CensusIndividual candidate) =>
            website.BirthYear > 0 && candidate.BirthDate.IsKnown && website.BirthYear == candidate.BirthDate.StartDate.Year;

        static string Normalise(string reference) =>
            new string(reference.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        /// <summary>
        /// Builds the Lost Cousins fact to record on a matched individual, mirroring the existing
        /// auto-detected-census-reference fact (Fact.LC_FTA, createdByFTA: true) used elsewhere.
        /// </summary>
        public static Fact CreateConfirmationFact(Match match) =>
            new(Fact.LC_FTA, match.Individual.CensusDate, match.Individual.CensusLocation,
                $"Lost Cousins fact created by FTAnalyzer after matching a My Ancestors page entry (reference: {match.WebsiteEntry.Reference})",
                false, true);

        public readonly record struct PossibleMatch(CensusIndividual Individual, LostCousin WebsiteEntry);

        /// <summary>
        /// For website entries Reconcile() couldn't match by reference, looks for a same-census,
        /// same-name, same-birth-year individual among those whose OWN census reference didn't parse
        /// locally (CensusReference.Status != GOOD - the Missing/Incomplete/Unrecognised buckets on
        /// the Census References report). Deliberately a lower-confidence, name/birth-year-only match
        /// - there's no reference left to cross-check, since a missing/unusable reference is exactly
        /// why the candidate is in this pool - so this is surfaced as a "probably them, go add a
        /// citation to confirm" hint rather than an automatic match, and (unlike Reconcile's matches)
        /// is never used to create a Lost Cousins confirmation fact.
        ///
        /// Tries progressively looser criteria, but only acts on a tier that leaves exactly one
        /// candidate for this website entry - e.g. two different "Andrew Bisset"s on the same census
        /// (same first forename, birth years a year or two apart) are exactly the case a first-
        /// forename-only match can't tell apart, and picking the wrong one sends the user to fix the
        /// wrong person's citation, which is worse than no suggestion at all.
        ///
        /// Every tier requires SurnamesMatch, even the full-name tiers below - a household member
        /// whose own citation isn't good enough to build a reference (why they're in the uncited pool
        /// in the first place) is being searched for across the WHOLE tree, not just their own
        /// household, so an unrelated person elsewhere in the tree who happens to share a forename and
        /// birth year (a married-name daughter, an unrelated same-name family) is a real risk without
        /// it - see the regression test for a real case this produced (a household's "Martha" wrongly
        /// suggested as an unrelated "Martha Pennington").
        /// </summary>
        public static List<PossibleMatch> FindPossibleMatches(
            IReadOnlyList<LostCousin> unmatchedWebsiteEntries, IReadOnlyList<CensusIndividual> allCensusIndividuals)
        {
            List<CensusIndividual> uncited = [.. allCensusIndividuals.Where(c =>
                c.CensusReference is null || c.CensusReference.Status != CensusReference.ReferenceStatus.GOOD)];
            CandidateNameCache cache = new();

            List<PossibleMatch> result = [];
            foreach (LostCousin website in unmatchedWebsiteEntries)
            {
                // FactDate/CensusDate's GetHashCode is reference-based while == is value-based (see
                // FindMatch above) - compare with the operator directly rather than a dictionary key.
                List<CensusIndividual> sameCensus = [.. uncited.Where(c =>
                    website.CensusDate is null || website.CensusDate == c.CensusDate)];

                CensusIndividual? match =
                    UniqueMatch(sameCensus, c => FullNameMatches(website, c) && SurnamesMatch(website, c, cache) && ExactBirthYear(website, c))
                    ?? UniqueMatch(sameCensus, c => FullNameMatches(website, c) && SurnamesMatch(website, c, cache) && BirthYearsAgree(website, c))
                    ?? UniqueMatch(sameCensus, c => NamesMatch(website, c, cache) && SurnamesMatch(website, c, cache) && BirthYearsAgree(website, c));
                if (match is not null)
                    result.Add(new PossibleMatch(match, website));
            }
            return result;
        }

        static CensusIndividual? UniqueMatch(List<CensusIndividual> candidates, Func<CensusIndividual, bool> predicate)
        {
            List<CensusIndividual> matches = [.. candidates.Where(predicate)];
            return matches.Count == 1 ? matches[0] : null;
        }
    }
}
