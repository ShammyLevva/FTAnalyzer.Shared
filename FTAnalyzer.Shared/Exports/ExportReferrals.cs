namespace FTAnalyzer
{
    public class ExportReferrals : IExportReferrals
    {
        Individual ind;
        Fact f;
        Fact censusFact;

        public ExportReferrals(Individual ind, Fact f)
        {
            this.ind = ind;
            this.f = f;
            censusFact = ind.GetCensusFact(f);
        }

        public string CensusReference
        {
            get
            {
                if (censusFact == null)
                    return "Census Not Found";
                return censusFact.CensusReference == null ? string.Empty : censusFact.CensusReference.Reference;
            }
        }
        public string IndividualID => ind.IndividualID;
        public string FamilyID => ind.ReferralFamilyID;
        public string Forenames => ind.Forenames;
        public string Surname => ind.Surname;
        public Age Age => ind.GetAge(f.FactDate);
        public string Census => censusFact == null ? f.ToString() : censusFact.ToString();
        public string CensusDate => f.FactDate.ToString();
        public bool Include => ind.IsBloodDirect;

        public string RelationType
        {
            get
            {
                if (ind.RelationType == Individual.DIRECT)
                    return Properties.Messages.Referral_Direct;
                if (ind.RelationType == Individual.BLOOD)
                    return Properties.Messages.Referral_Blood;
                if (ind.RelationType == Individual.MARRIEDTODB)
                    return Properties.Messages.Referral_Marriage;
                if (ind.RelationType == Individual.DESCENDANT)
                    return Properties.Messages.Referral_Descendant;
                else return string.Empty;
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
