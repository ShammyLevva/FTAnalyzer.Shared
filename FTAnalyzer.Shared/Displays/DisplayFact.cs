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
#endif
        public string Relation => Ind == null ? string.Empty : Ind.Relation;
        public string RelationToRoot => Ind == null ? string.Empty : Ind.RelationToRoot;
        public string SurnameAtDate => Ind == null ? string.Empty : Ind.SurnameAtDate(FactDate);
        public bool Preferred => Fact.Preferred;
        public bool IgnoreFact { get; set; }
        public string FactHash => Ind == null ? Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.GEDCOMLocation :
                                              Ind.IndividualID + Fact.Preferred + Fact.FactTypeDescription + Fact.DateString + Fact.Location.GEDCOMLocation;

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
            return Ind == null ? Fact.FactTypeDescription + ": " + Fact.FactDate + " " + Fact.Comment 
                               : IndividualID + ": " + Forenames + " " + Surname + ", " + Fact.ToString();
        }
    }
}
