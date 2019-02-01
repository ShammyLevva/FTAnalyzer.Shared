using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DataError : IColumnComparer<DataError>, IDisplayDataError
    {
        public DataError(int errorType, Fact.FactError errorLevel, Individual ind, string description)
        {
            ErrorType = DataErrorGroup.ErrorDescription(errorType);
#if __PC__
            Icon = FactImage.ErrorIcon(errorLevel).Icon;
#elif __MACOS__
            if (errorLevel == Fact.FactError.ERROR) 
                System.Console.WriteLine("nothing"); // stop compiler warning
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
        Family family;

#if __PC__
        public System.Drawing.Image Icon { get; private set; }
#endif
        public string ErrorType { get; private set; }
        public string Reference => individual == null ? family.FamilyID : individual.IndividualID;
        public string Name => individual == null ? family.FamilyName : individual.Name;
        public string Forenames => individual == null ? family.Forenames : individual.Forenames;
        public string Surname => individual == null ? family.Surname : individual.Surname;
        public string Description { get; private set; }
        public FactDate Born => individual == null ? FactDate.UNKNOWN_DATE : individual.BirthDate;
        public FactDate Died => individual == null ? FactDate.UNKNOWN_DATE : individual.DeathDate;
        
#if __PC__
        public bool IsFamily => individual == null;
#elif __MACOS__ || __IOS__
        public string IsFamily => individual == null ? "Yes" : "No";
#endif
        public IComparer<DataError> GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "ErrorType": return CompareComparableProperty<DataError>(f => f.ErrorType, ascending);
                case "Reference": return CompareComparableProperty<DataError>(f => f.Reference, ascending);
                case "Name": return CompareComparableProperty<DataError>(f => f.Name, ascending);
                case "Description": return CompareComparableProperty<DataError>(f => f.Description, ascending);
                case "Born": return CompareComparableProperty<DataError>(f => f.Born, ascending);
                case "Died": return CompareComparableProperty<DataError>(f => f.Died, ascending);
                case "IsFamily": return CompareComparableProperty<DataError>(f => f.IsFamily, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareComparableProperty<T>(Func<DataError, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as DataError);
                var c2 = accessor(y as DataError);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}