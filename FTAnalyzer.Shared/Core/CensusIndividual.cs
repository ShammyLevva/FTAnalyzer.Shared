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

        public int FamilyMembersCount => Family.Members.Count<CensusIndividual>();

        public string FamilyID => Family.FamilyID;

        public FactLocation CensusLocation => IsCensusDone(CensusDate) ? BestLocation(CensusDate) : Family.BestLocation;

        public CensusDate CensusDate => Family.CensusDate;

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

        public Age Age => GetAge(CensusDate);

        public string CensusSurname => Family.Surname;

        public string CensusReference
        {
            get
            {
                foreach (Fact f in AllFacts)
                    if (f.IsValidCensus(CensusDate) && f.CensusReference != null)
                        return f.CensusReference.Reference;
                return string.Empty;
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
