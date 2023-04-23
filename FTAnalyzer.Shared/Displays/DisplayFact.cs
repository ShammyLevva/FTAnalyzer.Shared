using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DisplayFact : IDisplayFact, IComparable
    {
        public string Surname { get; private set; }
        public string Forenames { get; private set; }
        public Individual? Ind { get; private set; }
        public Fact Fact { get; set; }
        public bool IgnoreFact { get; set; }

#if __PC__
        public Image Icon { get; private set; }
        public Color BackColour { get; set; }
#endif
        public DisplayFact(Individual? ind, Fact fact) : this(ind, ind.Surname, ind.Forenames, fact) { }
        public DisplayFact(Individual? ind, string surname, string forenames, Fact fact)
        {
            Ind = ind;
            Surname = surname;
            Forenames = forenames;
            Fact = fact;
            IgnoreFact = false;
#if __PC__
            Icon = GraphicsUtilities.ResizeImageToCurrentScale(FactImage.ErrorIcon(fact.FactErrorLevel).Icon);
#endif
        }
        public FactDate DateofBirth => Ind is null ? FactDate.UNKNOWN_DATE : Ind.BirthDate;
        public string TypeOfFact => Fact.FactTypeDescription;
        public FactDate FactDate => Fact.FactDate;
        public FactLocation Location => Fact.Location;
        public IList<FactSource> Sources => Fact.Sources;
        public double Latitude => Fact.Location.Latitude;
        public double Longitude => Fact.Location.Longitude;
        public string Comment => Fact.Comment;
        public string IndividualID => Ind is null ? string.Empty : Ind.IndividualID;
        public Age AgeAtFact => Ind?.GetAge(Fact.FactDate, Fact.FactType);
        public int SourcesCount => Fact.SourcesCount;
        public string SourceList => Fact.SourceList;
        public CensusReference CensusReference => Fact.CensusReference;
        public string CensusRefYear => Fact.CensusReference.CensusYear.IsKnown ? Fact.CensusReference.CensusYear.ToString() : string.Empty;
        public string FoundLocation => Fact.Location.FoundLocation;
        public string FoundResultType => Fact.Location.FoundResultType;
        public string GeocodeStatus => Fact.Location.Geocoded;
#if __PC__
        public Image LocationIcon => GraphicsUtilities.ResizeImageToCurrentScale(FactLocationImage.ErrorIcon(Fact.Location.GeocodeStatus).Icon);
        public bool Preferred => Fact.Preferred;
        public bool IgnoredFact => IgnoreFact;
#elif __MACOS__ || __IOS__
        public string Preferred => Fact.Preferred ? "Yes" : "No";
        public string IgnoredFact => IgnoreFact ? "Yes" : "No";
#endif
        public string Relation => Ind is null ? string.Empty : Ind.Relation;
        public string RelationToRoot => Ind is null ? string.Empty : Ind.RelationToRoot;
        public string SurnameAtDate => Ind is null ? string.Empty : Ind.SurnameAtDate(FactDate);
        public string FactHash => Ind is null ? Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.OriginalText :
                                              Ind.IndividualID + Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.OriginalText;
        public string ErrorComment => Fact.FactErrorMessage;

        public int CompareTo(object? obj)
        {
            DisplayFact that = (DisplayFact)obj;
            return FactDate == that.FactDate && Ind is not null ? Ind.CompareTo(that.Ind) : FactDate.CompareTo(that.FactDate);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            DisplayFact that = (DisplayFact)obj;
            return FactHash.Equals(that.FactHash); //this.Ind.Equals(that.Ind) && this.Fact.Equals(that.Fact);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => 
            Ind is null ? $"{Fact.FactTypeDescription}: {Fact.FactDate} {Fact.Comment}"
                        : $"{IndividualID}: {Forenames} {Surname}, {Fact}";

        public IComparer<IDisplayFact> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "IndividualID" => CompareComparableProperty<IDisplayFact>(f => f.IndividualID, ascending),
                "Surname" => new NameComparer<IDisplayFact>(ascending, false),
                "Forenames" => new NameComparer<IDisplayFact>(ascending, true),
                "DateofBirth" => CompareComparableProperty<IDisplayFact>(f => f.DateofBirth, ascending),
                "SurnameAtDate" => CompareComparableProperty<IDisplayFact>(f => f.SurnameAtDate, ascending),
                "TypeOfFact" => CompareComparableProperty<IDisplayFact>(f => f.TypeOfFact, ascending),
                "FactDate" => CompareComparableProperty<IDisplayFact>(f => f.FactDate, ascending),
                "Relation" => CompareComparableProperty<IDisplayFact>(f => f.Relation, ascending),
                "RelationToRoot" => CompareComparableProperty<IDisplayFact>(f => f.RelationToRoot, ascending),
                "Location" => CompareComparableProperty<IDisplayFact>(f => f.Location, ascending),
                "AgeAtFact" => CompareComparableProperty<IDisplayFact>(f => f.AgeAtFact, ascending),
                "GeocodeStatus" => CompareComparableProperty<IDisplayFact>(f => f.GeocodeStatus, ascending),
                "FoundLocation" => CompareComparableProperty<IDisplayFact>(f => f.FoundLocation, ascending),
                "FoundResultType" => CompareComparableProperty<IDisplayFact>(f => f.FoundResultType, ascending),
                "CensusReference" => CompareComparableProperty<IDisplayFact>(f => f.CensusReference.ToString(), ascending),
                "CensusRefYear" => CompareComparableProperty<IDisplayFact>(f => f.CensusRefYear, ascending),
                "Comment" => CompareComparableProperty<IDisplayFact>(f => f.Comment, ascending),
                "SourceList" => CompareComparableProperty<IDisplayFact>(f => f.SourceList, ascending),
                "Latitude" => CompareComparableProperty<IDisplayFact>(f => f.Latitude, ascending),
                "Longitude" => CompareComparableProperty<IDisplayFact>(f => f.Longitude, ascending),
                "Preferred" => CompareComparableProperty<IDisplayFact>(f => f.Preferred, ascending),
                "IgnoredFact" => CompareComparableProperty<IDisplayFact>(f => f.IgnoredFact, ascending),
                _ => null,
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<DisplayFact, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as DisplayFact);
                var c2 = accessor(y as DisplayFact);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}
