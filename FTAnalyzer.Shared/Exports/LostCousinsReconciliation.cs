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
    /// </summary>
    public static class LostCousinsReconciliation
    {
        public readonly record struct Match(CensusIndividual Individual, LostCousin WebsiteEntry);

        public static (List<CensusIndividual> StillMissing, List<Match> ConfirmedOnWebsite) Reconcile(
            IReadOnlyList<LostCousin> websiteAncestors, IReadOnlyList<CensusIndividual> candidates)
        {
            List<CensusIndividual> stillMissing = [];
            List<Match> confirmed = [];

            Dictionary<string, List<LostCousin>> websiteByReference = websiteAncestors
                .Where(w => !string.IsNullOrWhiteSpace(w.Reference))
                .GroupBy(w => Normalise(w.Reference))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (CensusIndividual candidate in candidates)
            {
                LostCousin? match = FindMatch(candidate, websiteByReference);
                if (match is not null)
                    confirmed.Add(new Match(candidate, match));
                else
                    stillMissing.Add(candidate);
            }

            return (stillMissing, confirmed);
        }

        static LostCousin? FindMatch(CensusIndividual candidate, Dictionary<string, List<LostCousin>> websiteByReference)
        {
            // Lost Cousins' own website always shows references in compact slash form, regardless of
            // this user's "Use compact census references" display preference - compare like for like.
            // Built via LostCousinsCensusReference, not candidate.CompactCensusRef directly, since
            // Lost Cousins' own reference doesn't always contain the same fields CensusReference's
            // general-purpose display string does (see LostCousinsCensusReference for why).
            string candidateRef = LostCousinsCensusReference.Build(candidate.CensusReference);
            if (string.IsNullOrWhiteSpace(candidateRef))
                return null;
            if (!websiteByReference.TryGetValue(Normalise(candidateRef), out List<LostCousin>? group))
                return null;

            // FactDate/CensusDate's GetHashCode is reference-based while == is value-based, so
            // compare with the operator directly rather than trusting it as a dictionary key.
            List<LostCousin> sameCensus = [.. group.Where(w => w.CensusDate is null || w.CensusDate == candidate.CensusDate)];
            if (sameCensus.Count == 1)
                return sameCensus[0];

            return sameCensus.FirstOrDefault(w => NamesMatch(w, candidate) && BirthYearsAgree(w, candidate));
        }

        static bool BirthYearsAgree(LostCousin website, CensusIndividual candidate) =>
            website.BirthYear <= 0 || !candidate.BirthDate.IsKnown || Math.Abs(website.BirthYear - candidate.BirthDate.StartDate.Year) <= 5;

        static bool NamesMatch(LostCousin website, CensusIndividual candidate)
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
            return new DoubleMetaphone(webForename).PrimaryKey == new DoubleMetaphone(candidate.LCForename).PrimaryKey;
        }

        // Only needed for FindPossibleMatches below - Reconcile's own FindMatch never needs a surname
        // check because the census reference it already matched on identifies the household (and
        // therefore the surname) implicitly; forename alone then disambiguates household members.
        // FindPossibleMatches has no reference to narrow by, so without this it would happily match
        // any same-forename, same-birth-year person on the same census anywhere in the tree.
        static bool SurnamesMatch(LostCousin website, CensusIndividual candidate)
        {
            string webSurname = ExtractSurname(website.Name);
            string candidateSurname = candidate.CensusSurname;
            if (string.IsNullOrEmpty(webSurname) || string.IsNullOrEmpty(candidateSurname))
                return false;
            if (string.Equals(webSurname, candidateSurname, StringComparison.OrdinalIgnoreCase))
                return true;
            return new DoubleMetaphone(webSurname).PrimaryKey == new DoubleMetaphone(candidateSurname).PrimaryKey;
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
        /// </summary>
        public static List<PossibleMatch> FindPossibleMatches(
            IReadOnlyList<LostCousin> unmatchedWebsiteEntries, IReadOnlyList<CensusIndividual> allCensusIndividuals)
        {
            List<CensusIndividual> uncited = [.. allCensusIndividuals.Where(c =>
                c.CensusReference is null || c.CensusReference.Status != CensusReference.ReferenceStatus.GOOD)];

            List<PossibleMatch> result = [];
            foreach (LostCousin website in unmatchedWebsiteEntries)
            {
                // FactDate/CensusDate's GetHashCode is reference-based while == is value-based (see
                // FindMatch above) - compare with the operator directly rather than a dictionary key.
                List<CensusIndividual> sameCensus = [.. uncited.Where(c =>
                    website.CensusDate is null || website.CensusDate == c.CensusDate)];

                CensusIndividual? match =
                    UniqueMatch(sameCensus, c => FullNameMatches(website, c) && ExactBirthYear(website, c))
                    ?? UniqueMatch(sameCensus, c => FullNameMatches(website, c) && BirthYearsAgree(website, c))
                    ?? UniqueMatch(sameCensus, c => NamesMatch(website, c) && SurnamesMatch(website, c) && BirthYearsAgree(website, c));
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
