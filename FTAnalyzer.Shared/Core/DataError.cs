using FTAnalyzer.Utilities;

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
            if (errorLevel == Fact.FactError.ERROR) 
                Console.WriteLine("nothing"); // stop compiler warning
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
        [ColumnDetail("Icon", 50)]
        public System.Drawing.Image Icon { get; private set; }
#endif
        [ColumnDetail("Error Type", 200)]
        public string ErrorType { get; private set; }
        [ColumnDetail("Ref", 60)]
        public string Reference => individual == null ? family.FamilyID : individual.IndividualID;
        [ColumnDetail("Name", 200)]
        public string Name { get { return individual == null ? family.FamilyName : individual.Name; } }
        [ColumnDetail("Description", 400)]
        public string Description { get; private set; }
        [ColumnDetail("Born", 150)]
        public FactDate Born => individual == null ? FactDate.UNKNOWN_DATE : individual.BirthDate;
        [ColumnDetail("Died", 150)]
        public FactDate Died => individual == null ? FactDate.UNKNOWN_DATE : individual.DeathDate;
#if __PC__
        [ColumnDetail("Family", 50)]
        public bool IsFamily => individual == null;
#elif __MACOS__
        [ColumnDetail("Family", 50)]
        public string IsFamily => individual == null ? "Yes" : "No";
#endif
    }
}
