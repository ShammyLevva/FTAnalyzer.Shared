using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DataError : IDisplayDataError
    {
        public DataError(int errorType, Fact.FactError errorLevel, Individual ind, string description)
        {
            ErrorType = DataErrorGroup.ErrorDescription(errorType);
#if __PC__
            Icon = FactImage.ErrorIcon(errorLevel).Icon;
#elif __MACOS__
            if (errorLevel == Fact.FactError.ERROR) 
                System.Debug.WriteLine("nothing"); // stop compiler warning
#endif
            individual = ind;
            family = null;
            Description = description;
        }
        
        public DataError(int errorType, Individual ind, string description)
            : this(errorType, Fact.FactError.ERROR, ind, description) {}

        public DataError(int errorType, Family fam, string description)
            : this(errorType, Fact.FactError.ERROR, null, description)
        {
            family = fam;
        }

        readonly Individual individual;
        readonly Family family;

#if __PC__
        public System.Drawing.Image Icon { get; private set; }
#endif
        public string ErrorType { get; private set; }
        public string Reference => individual is null ? family.FamilyID : individual.IndividualID;
        public string Name => individual is null ? family.FamilyName : individual.Name;
        public string Forenames => individual is null ? family.Forenames : individual.Forenames;
        public string Surname => individual is null ? family.Surname : individual.Surname;
        public string Description { get; private set; }
        public FactDate Born => individual is null ? FactDate.UNKNOWN_DATE : individual.BirthDate;
        public FactDate Died => individual is null ? FactDate.UNKNOWN_DATE : individual.DeathDate;
        
#if __PC__
        public bool IsFamily => individual is null;
#elif __MACOS__ || __IOS__
        public string IsFamily => individual is null ? "Yes" : "No";
#endif
        public IComparer<IDisplayDataError> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "ErrorType" => CompareComparableProperty<IDisplayDataError>(f => f.ErrorType, ascending),
                "Reference" => CompareComparableProperty<IDisplayDataError>(f => f.Reference, ascending),
                "Name" => CompareComparableProperty<IDisplayDataError>(f => f.Name, ascending),
                "Description" => CompareComparableProperty<IDisplayDataError>(f => f.Description, ascending),
                "Born" => CompareComparableProperty<IDisplayDataError>(f => f.Born, ascending),
                "Died" => CompareComparableProperty<IDisplayDataError>(f => f.Died, ascending),
                //case "IsFamily": return CompareComparableProperty<IDisplayDataError>(f => f.IsFamily, ascending);
                _ => CompareComparableProperty<IDisplayDataError>(f => f.ErrorType, ascending),
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplayDataError, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as IDisplayDataError);
                var c2 = accessor(y as IDisplayDataError);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}