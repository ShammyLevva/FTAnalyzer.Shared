using System.Linq;

namespace FTAnalyzer
{
    public class CensusIndividual : Individual, IDisplayCensus
    {
        public static string HUSBAND = "Husband", WIFE = "Wife", CHILD = "Child", UNKNOWNSTATUS = "Unknown";
        
        CensusFamily Family;
        public int Position { get; private set; }
        public string CensusStatus { get; private set; }

        public CensusIndividual(int position, Individual individual, CensusFamily family, string censusStatus)
            : base(individual)
        {
            Position = position;
            Family = family;
            CensusStatus = censusStatus;
        }

        public int FamilyMembersCount => Family.Members.Count();
        public string FamilyID => Family.FamilyID;
        public FactLocation CensusLocation => IsCensusDone(CensusDate) ? BestLocation(CensusDate) : Family.BestLocation;
        public string CensusRef => CensusReference == null ? string.Empty : CensusReference.Reference.Trim();
        public CensusDate CensusDate => Family.CensusDate;
        public Age Age => GetAge(CensusDate);
        public string CensusSurname => Family.Surname;
        public bool IsKnownCensusReference => CensusReference != null && CensusReference.IsKnownStatus;

        public string LCAge
        {
            get
            {
                if (Age.MinAge > 0)
                    return Age.MinAge.ToString();
                if (Age.MaxAge > 0)
                    return Age.MaxAge.ToString();
                if (BirthDate.IsKnown)
                {
                    int months = BirthDate.MonthsDifference(CensusDate);
                    return $"{months}m";
                }
                else
                    return "0";
            }
        }

        public string CensusName
        {
            get
            {
                if (CensusStatus == WIFE)
                {
                    string surname = Surname.Length > 0 ? $" ({Surname})" : string.Empty;
                    return $"{Forenames} {MarriedName} {surname}";
                }
                return Name;
            }
        }

        public CensusReference CensusReference
        {
            get
            {
                foreach (Fact f in AllFacts)
                    if (f.FactDate.Overlaps(CensusDate) && f.IsValidCensus(CensusDate) && f.CensusReference != null)
                        return f.CensusReference;
                return null;
            }
        }
#if __PC__
        public System.Windows.Forms.DataGridViewCellStyle CellStyle { get; set; }
#endif
        public bool IsValidLocation(string location) => 
            !CensusLocation.IsKnownCountry || Countries.IsUnitedKingdom(location) ? CensusLocation.IsUnitedKingdom : CensusLocation.Country.Equals(location);

        public override string ToString() => $"{IndividualID}: {Name} b.{BirthDate}";
    }
}
