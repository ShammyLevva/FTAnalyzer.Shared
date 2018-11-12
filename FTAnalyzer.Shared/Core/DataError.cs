using FTAnalyzer.Shared.Utilities;

namespace FTAnalyzer
{
    public class DataError
    {
        public DataError(int errorType, Fact.FactError errorLevel, Individual ind, string description)
        {
            ErrorType = DataErrorGroup.ErrorDescription(errorType);
#if __PC__
            Icon = FactImage.ErrorIcon(errorLevel).Icon;
#elif __MACOS__
            var x = errorLevel; // stop compiler warning
#endif
            individual = ind;
            family = null;
            Description = description;
        }
        
        public DataError(int errorType, Individual ind, string description)
            : this(errorType, Fact.FactError.ERROR, ind, description) {}

        public DataError(int errorType, Family fam, string description)
            : this(errorType, Fact.FactError.ERROR, (Individual)null, description)
        {
            family = fam;
        }
                    
        Individual individual;
        Family family;

#if __PC__
        [ColumnWidth(5)]
        public System.Drawing.Image Icon { get; private set; }
#endif
        [ColumnWidth(30)]
        public string ErrorType { get; private set; }
        [ColumnWidth(20)]
        public string Reference => individual == null ? family.FamilyID : individual.IndividualID;
        [ColumnWidth(30)]
        public string Name { get { return individual == null ? family.FamilyName : individual.Name; } }
        [ColumnWidth(100)]
        public string Description { get; private set; }
        [ColumnWidth(30)]
        public FactDate Born => individual == null ? FactDate.UNKNOWN_DATE : individual.BirthDate;
        [ColumnWidth(30)]
        public FactDate Died => individual == null ? FactDate.UNKNOWN_DATE : individual.DeathDate;
        [ColumnWidth(20)]
        public bool IsFamily() => individual == null;
    }
}
