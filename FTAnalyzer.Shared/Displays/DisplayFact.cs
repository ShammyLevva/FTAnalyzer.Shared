using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DisplayFact : IDisplayFact, IComparable
    {
        public string Surname { get; private set; }
        public string Forenames { get; private set; }
        public Individual Ind { get; private set; }
        public Fact Fact { get; set; }
        public bool IgnoreFact { get; set; }

#if __PC__
        public System.Drawing.Image Icon { get; private set; }
        public System.Drawing.Color BackColour { get; set; }
#endif
        public DisplayFact(Individual ind, Fact fact) : this(ind, ind.Surname, ind.Forenames, fact) { }
        public DisplayFact(Individual ind, string surname, string forenames, Fact fact)
        {
            Ind = ind;
            Surname = surname;
            Forenames = forenames;
            Fact = fact;
            IgnoreFact = false;
#if __PC__
            Icon = FactImage.ErrorIcon(fact.FactErrorLevel).Icon;
#endif
        }
        public FactDate DateofBirth => Ind == null ? FactDate.UNKNOWN_DATE : Ind.BirthDate;
        public string TypeOfFact => Fact.FactTypeDescription;
        public FactDate FactDate => Fact.FactDate;
        public FactLocation Location => Fact.Location;
        public IList<FactSource> Sources => Fact.Sources;
        public double Latitude => Fact.Location.Latitude;
        public double Longitude => Fact.Location.Longitude;
        public string Comment => Fact.Comment;
        public string IndividualID => Ind == null ? string.Empty : Ind.IndividualID;
        public Age AgeAtFact => Ind?.GetAge(Fact.FactDate, Fact.FactType);
        public string SourceList => Fact.SourceList;
        public CensusReference CensusReference => Fact.CensusReference;
        public string CensusRefYear => Fact.CensusReference.CensusYear.IsKnown ? Fact.CensusReference.CensusYear.ToString() : string.Empty;
        public string FoundLocation => Fact.Location.FoundLocation;
        public string FoundResultType => Fact.Location.FoundResultType;
        public string GeocodeStatus => Fact.Location.Geocoded;
#if __PC__
        public System.Drawing.Image LocationIcon => FactLocationImage.ErrorIcon(Fact.Location.GeocodeStatus).Icon;
        public bool Preferred => Fact.Preferred;
        public bool IgnoredFact => IgnoreFact;
#elif __MACOS__
        public string Preferred => Fact.Preferred ? "Yes" : "No";
        public string IgnoredFact => IgnoreFact ? "Yes" : "No";
#endif
        public string Relation => Ind == null ? string.Empty : Ind.Relation;
        public string RelationToRoot => Ind == null ? string.Empty : Ind.RelationToRoot;
        public string SurnameAtDate => Ind == null ? string.Empty : Ind.SurnameAtDate(FactDate);
        public string FactHash => Ind == null ? Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.OriginalText :
                                              Ind.IndividualID + Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.OriginalText;

        public int CompareTo(object obj)
        {
            DisplayFact that = (DisplayFact)obj;
            return FactDate == that.FactDate && Ind != null ? Ind.CompareTo(that.Ind) : FactDate.CompareTo(that.FactDate);
        }

        public override bool Equals(object obj)
        {
            DisplayFact that = (DisplayFact)obj;
            return FactHash.Equals(that.FactHash); //this.Ind.Equals(that.Ind) && this.Fact.Equals(that.Fact);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return Ind == null ? $"{Fact.FactTypeDescription}: {Fact.FactDate} {Fact.Comment}" 
                               : $"{IndividualID}: {Forenames} {Surname}, {Fact}";
        }

        public IComparer<IDisplayFact> GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareStringProperty<IDisplayFact>(f => f.IndividualID, ascending);
                case "Surname": return new NameComparer<IDisplayFact>(ascending, false);
                case "Forenames": return new NameComparer<IDisplayFact>(ascending, true);
                case "DateofBirth": return CompareDateProperty<IDisplayFact>(f => f.DateofBirth, ascending);
                case "SurnameAtDate": return CompareStringProperty<IDisplayFact>(f => f.SurnameAtDate, ascending);
                case "TypeOfFact": return CompareStringProperty<IDisplayFact>(f => f.TypeOfFact, ascending);
                case "FactDate": return CompareDateProperty<IDisplayFact>(f => f.FactDate, ascending);
                case "Relation": return CompareStringProperty<IDisplayFact>(f => f.Relation, ascending);
                case "RelationToRoot": return CompareStringProperty<IDisplayFact>(f => f.RelationToRoot, ascending);
                case "Location": return CompareLocationProperty<IDisplayFact>(f => f.Location, ascending);
                case "AgeAtFact": return CompareAgeProperty<IDisplayFact>(f => f.AgeAtFact, ascending);
                case "GeocodeStatus": return CompareStringProperty<IDisplayFact>(f => f.GeocodeStatus, ascending);
                case "FoundLocation": return CompareStringProperty<IDisplayFact>(f => f.FoundLocation, ascending);
                case "FoundResultType": return CompareStringProperty<IDisplayFact>(f => f.FoundResultType, ascending);
                case "CensusReference": return CompareStringProperty<IDisplayFact>(f => f.CensusReference.ToString(), ascending);
                case "CensusRefYear": return CompareStringProperty<IDisplayFact>(f => f.CensusRefYear, ascending);
                case "Comment": return CompareStringProperty<IDisplayFact>(f => f.Comment, ascending);
                case "SourceList": return CompareStringProperty<IDisplayFact>(f => f.SourceList, ascending);
                case "Latitude": return CompareDoubleProperty<IDisplayFact>(f => f.Latitude, ascending);
                case "Longitude": return CompareDoubleProperty<IDisplayFact>(f => f.Longitude, ascending);
                case "Preferred": return CompareStringProperty<IDisplayFact>(f => f.Preferred, ascending);
                case "IgnoredFact": return CompareStringProperty<IDisplayFact>(f => f.IgnoredFact, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareStringProperty<T>(Func<DisplayFact, string> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var s1 = accessor(x as DisplayFact);
                var s2 = accessor(y as DisplayFact);
                var result = string.Compare(s1, s2);
                return ascending ? result : -result;
            });
        }

        Comparer<T> CompareDoubleProperty<T>(Func<DisplayFact, double> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var d1 = accessor(x as DisplayFact);
                var d2 = accessor(y as DisplayFact);
                double result = ascending ? d1 - d2 : d2 - d1;
                return Math.Sign(result);
            });
        }

        Comparer<T> CompareDateProperty<T>(Func<DisplayFact, FactDate> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var fd1 = accessor(x as DisplayFact);
                var fd2 = accessor(y as DisplayFact);
                int result = fd1.CompareTo(fd2);
                return ascending ? result : -result;
            });
        }

        Comparer<T> CompareLocationProperty<T>(Func<DisplayFact, FactLocation> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var fl1 = accessor(x as DisplayFact);
                var fl2 = accessor(y as DisplayFact);
                int result = fl1.CompareTo(fl2);
                return ascending ? result : -result;
            });
        }

        Comparer<T> CompareAgeProperty<T>(Func<DisplayFact, Age> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var a1 = accessor(x as DisplayFact);
                var a2 = accessor(y as DisplayFact);
                int result = a1.CompareTo(a2);
                return ascending ? result : -result;
            });
        }
    }
}
