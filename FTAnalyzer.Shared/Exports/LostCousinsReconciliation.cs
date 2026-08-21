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
            readonly Dictionary<CensusIndividual, string> _maidenSurnameMetaphone = [];

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

            public string MaidenSurnameMetaphone(CensusIndividual c)
            {
                if (!_maidenSurnameMetaphone.TryGetValue(c, out string? key))
                    _maidenSurnameMetaphone[c] = key = new DoubleMetaphone(c.Surname).PrimaryKey;
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
            // entry - most commonly every member of one household sharing that household's own
            // reference (FindMatchByReference's sameCensus.Count==1 shortcut hands the sole website
            // entry back to whichever household member asks first, with no name check at all) but
            // also a shared reference spanning more than one household (see class doc comment).
            // Rather than let an arbitrary one win, prefer whichever candidate's name actually
            // agrees with the website entry's; the loser goes back into stillMissing instead of
            // keeping a wrong "confirmed" match - which, via CreateConfirmationFact, would otherwise
            // write a fabricated Lost Cousins fact onto the wrong person.
            List<Match> confirmed = [.. allMatches
                .GroupBy(m => m.WebsiteEntry)
                .Select(g => g.OrderByDescending(m => MatchQuality(m, cache)).First())];

            HashSet<CensusIndividual> confirmedIndividuals = [.. confirmed.Select(m => m.Individual)];
            stillMissing.AddRange(allMatches.Select(m => m.Individual).Where(i => !confirmedIndividuals.Contains(i)));

            return (stillMissing, confirmed);
        }

        // Ranks a candidate's fit against the website entry it matched, for picking the right one
        // out of a household sharing a reference. Surname alone isn't enough to tell family members
        // apart - a father and son (or any two people sharing a surname) tie on it every time, which
        // previously let the household head win by iteration order regardless of whose forename the
        // website entry actually named (see Reconcile_PicksHouseholdMemberWhoseNameActuallyMatches,
        // a real report: "Southern, Peter" matched to his father John purely because John was
        // processed first). Forename+surname agreement outranks surname alone, which in turn
        // outranks neither - birth year only breaks a tie between two same-forename candidates
        // (e.g. a father and son who share both names).
        static int MatchQuality(Match match, CandidateNameCache cache)
        {
            bool names = NamesMatch(match.WebsiteEntry, match.Individual, cache);
            bool surnames = SurnamesMatch(match.WebsiteEntry, match.Individual, cache);
            if (names && surnames)
                return BirthYearsAgree(match.WebsiteEntry, match.Individual) ? 3 : 2;
            return surnames ? 1 : 0;
        }

        static LostCousin? FindMatch(CensusIndividual candidate, Dictionary<string, List<LostCousin>> websiteByReference, CandidateNameCache cache)
        {
            // Lost Cousins pins every household member to the head of household's own reference, even
            // for someone whose own citation correctly captured a different (typically later) census
            // page the household overflowed onto - so the head's reference (HouseholdCensusReference)
            // is tried first, exactly as before.
            string householdRef = LostCousinsCensusReference.Build(candidate.HouseholdCensusReference);
            LostCousin? match = FindMatchByReference(candidate, householdRef, websiteByReference, cache);
            if (match is not null)
                return match;

            // CensusFamily.HeadOfHousehold picks whoever is simply alive at the census date (Husband,
            // else Wife, else eldest child) - it has no way to check that person was actually
            // enumerated WITH this family that year. A parent who'd remarried and was living in a
            // completely different household by the census date is still picked as head, silently
            // overriding every child's own correct reference with an unrelated one - a real case, not
            // a hypothetical (see LostCousinsReconciliationTest.
            // Reconcile_FallsBackToOwnReferenceWhenHeadOfHouseholdWasElsewhereThatCensus).
            // Falling back to the individual's own reference here means a genuine match isn't thrown
            // away just because the household-reference guess didn't hold for this person - the
            // surname/name/birth-year checks below still guard against a coincidental collision, same
            // as they do for the household-reference attempt above.
            string ownRef = LostCousinsCensusReference.Build(candidate.CensusReference);
            return ownRef == householdRef ? null : FindMatchByReference(candidate, ownRef, websiteByReference, cache);
        }

        static LostCousin? FindMatchByReference(CensusIndividual candidate, string candidateRef,
            Dictionary<string, List<LostCousin>> websiteByReference, CandidateNameCache cache)
        {
            // Lost Cousins' own website always shows references in compact slash form, regardless of
            // this user's "Use compact census references" display preference - compare like for like.
            // Built via LostCousinsCensusReference, not candidate.CompactCensusRef directly, since
            // Lost Cousins' own reference doesn't always contain the same fields CensusReference's
            // general-purpose display string does (see LostCousinsCensusReference for why).
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
            if (string.IsNullOrEmpty(webSurname))
                return false;

            string marriedSurname = cache.SurnameAtDate(candidate);
            if (SurnameFormMatches(webSurname, marriedSurname, website.SurnameMetaphone, cache.SurnameMetaphone(candidate)))
                return true;

            // Lost Cousins doesn't require a member to enter a woman's surname exactly as it
            // appears on the census - some members correct an entry to her maiden name (as
            // recorded in their own family tree), even though the census itself, and this
            // candidate's own citation, record her married name at that date. A real report: "Jane
            // Bassett" on the 1911 census, corrected to "Jane Smith" (her maiden name) on Lost
            // Cousins, with a perfect reference match to her husband - the reference-based match
            // succeeded, but the surname check rejected it because Bassett/Smith share no sound in
            // common. Falling back to her own birth surname here means a genuinely correct
            // reference match isn't rejected just because of that choice. Skipped when it's the
            // same string as the married name already checked above (the common case - avoids
            // rebuilding an identical metaphone key for nothing).
            string maidenSurname = candidate.Surname;
            return !string.Equals(maidenSurname, marriedSurname, StringComparison.OrdinalIgnoreCase) &&
                SurnameFormMatches(webSurname, maidenSurname, website.SurnameMetaphone, cache.MaidenSurnameMetaphone(candidate));
        }

        static bool SurnameFormMatches(string webSurname, string candidateSurname, string webMetaphone, string candidateMetaphone)
        {
            if (string.IsNullOrEmpty(candidateSurname))
                return false;
            if (string.Equals(webSurname, candidateSurname, StringComparison.OrdinalIgnoreCase))
                return true;
            return webMetaphone == candidateMetaphone;
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
