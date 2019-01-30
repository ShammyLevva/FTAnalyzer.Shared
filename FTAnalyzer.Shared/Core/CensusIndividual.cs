using System;
using System.Collections.Generic;
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
        public string CensusCountry => CensusReference == null ? string.Empty : CensusReference.Country;
        public string Census => CensusDate.ToString();

        public string LCAge
        {
            get
            {
                if(Age.MaxAge - Age.MinAge <= 2) // range is tight in years
                    return ((Age.MinAge+Age.MaxAge)/2).ToString();
                if (Age.MinAge > 0)
                    return Age.MinAge.ToString();
                if (Age.MaxAge > 0)
                    return Age.MaxAge.ToString();
                if (BirthDate.IsKnown)
                {
                    int months = BirthDate.MonthsDifference(CensusDate);
                    return $"{months}m";
                }
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

        IComparer<IDisplayCensus> IColumnComparer<IDisplayCensus>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "FamilyID": return CompareComparableProperty<IDisplayCensus>(f => f.FamilyID, ascending);
                case "Position": return CompareComparableProperty<IDisplayCensus>(f => f.Position, ascending);
                case "IndividualID": return CompareComparableProperty<IDisplayCensus>(f => f.IndividualID, ascending);
                case "CensusLocation": return CompareComparableProperty<IDisplayCensus>(f => f.CensusLocation, ascending);
                case "CensusName": return CompareComparableProperty<IDisplayCensus>(f => f.CensusName, ascending);
                case "Age": return CompareComparableProperty<IDisplayCensus>(f => f.Age, ascending);
                case "Occupation": return CompareComparableProperty<IDisplayCensus>(f => f.Occupation, ascending);
                case "BirthDate": return CompareComparableProperty<IDisplayCensus>(f => f.BirthDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayCensus>(f => f.BirthLocation, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayCensus>(f => f.DeathDate, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayCensus>(f => f.DeathLocation, ascending);
                case "CensusStatus": return CompareComparableProperty<IDisplayCensus>(f => f.CensusStatus, ascending);
                case "CensusRef": return CompareComparableProperty<IDisplayCensus>(f => f.CensusRef, ascending);
                case "Relation": return CompareComparableProperty<IDisplayCensus>(f => f.Relation, ascending);
                case "RelationToRoot": return CompareComparableProperty<IDisplayCensus>(f => f.RelationToRoot, ascending);
                case "Ahnentafel": return CompareComparableProperty<IDisplayCensus>(f => f.Ahnentafel, ascending);
                default: return null;
            }

        }

        Comparer<T> CompareComparableProperty<T>(Func<IDisplayCensus, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var a = accessor(x as IDisplayCensus);
                var b = accessor(y as IDisplayCensus);
                int result = a.CompareTo(b);
                return ascending ? result : -result;
            });
        }
    }
}
