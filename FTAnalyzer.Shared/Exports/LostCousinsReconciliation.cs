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
            string candidateRef = candidate.CompactCensusRef;
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

        // LostCousin.Name is "Surname, Forename(s)" (see LostCousin.SetMetaphones).
        static string ExtractForename(string name)
        {
            int comma = name.IndexOf(',');
            if (comma < 0 || comma + 2 > name.Length)
                return name;
            string forenames = name[(comma + 2)..];
            int space = forenames.IndexOf(' ');
            return space > 0 ? forenames[..space] : forenames;
        }

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
    }
}
