namespace FTAnalyzer
{
    public class ExportReferrals : IExportReferrals
    {
        Individual Individual;
        Fact Fact;
        readonly Fact censusFact;

        public ExportReferrals(Individual ind, Fact f)
        {
            Individual = ind;
            Fact = f;
            censusFact = ind.GetCensusFact(f);
        }

        public string CensusReference => censusFact == null
                    ? "Census Not Found"
                    : censusFact.CensusReference == null ? string.Empty : censusFact.CensusReference.Reference;
        public string IndividualID => Individual.IndividualID;
        public string FamilyID => Individual.ReferralFamilyID;
        public string Forenames => Individual.Forenames;
        public string Surname => Individual.Surname;
        public Age Age => Individual.GetAge(Fact.FactDate);
        public string Census => censusFact == null ? Fact.ToString() : censusFact.ToString();
        public string CensusDate => Fact.FactDate.ToString();
        public bool Include => Individual.IsBloodDirect;

        public string RelationType
        {
            get
            {
                if (Individual.RelationType == Individual.DIRECT)
                    return "Direct Ancestor";
                if (Individual.RelationType == Individual.BLOOD)
                    return "Blood Relation";
                if (Individual.RelationType == Individual.MARRIEDTODB || Individual.RelationType == Individual.MARRIAGE)
                    return "Marriage";
                if (Individual.RelationType == Individual.DESCENDANT)
                    return "Descendant";
                if (Individual.RelationType == Individual.LINKED)
                    return "Linked";
                if (Individual.RelationType == Individual.UNKNOWN)
                    return "Unknown";
                return string.Empty;
            }
        }

        public string ShortCode
        {
            get
            {
                if (censusFact == null)
                    return string.Empty;
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.EWCENSUS1881) && censusFact.Location.IsEnglandWales)
                    return "RG11";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.SCOTCENSUS1881) && censusFact.Location.Equals(Countries.SCOTLAND))
                    return "SCT1";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.CANADACENSUS1881) && censusFact.Location.Equals(Countries.CANADA))
                    return "CAN1";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.USCENSUS1880) && censusFact.Location.Equals(Countries.UNITED_STATES))
                    return "USA1";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.EWCENSUS1841) && censusFact.Location.IsEnglandWales)
                    return "1841";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.IRELANDCENSUS1911) && censusFact.Location.Equals(Countries.IRELAND))
                    return "0IRL";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.EWCENSUS1911) && censusFact.Location.IsEnglandWales)
                    return "0ENG";
                if (censusFact.FactDate.Overlaps(FTAnalyzer.CensusDate.USCENSUS1940) && censusFact.Location.Equals(Countries.UNITED_STATES))
                    return "USA4";
                return string.Empty;
            }
        }
    }
}
