using System;

namespace FTAnalyzer
{
    public class LostCousin : IEquatable<LostCousin>
    {
        public string Name { get; }
        public int BirthYear { get; }
        public string Reference { get; }
        public CensusDate CensusDate { get; }
        public bool FTAnalyzerFact { get; }
        string SurnameMetaphone { get; }
        string ForenamesMetaphone { get; }
        

        public LostCousin(string name, int birthYear, string reference, int censusYear, string censusCountry, bool ftanalyzer)
        {
            Name = name;
            BirthYear = birthYear;
            Reference = FixReference(reference);
            FTAnalyzerFact = ftanalyzer;
            if (censusYear == 1841 && Countries.IsEnglandWales(censusCountry))
                CensusDate = CensusDate.EWCENSUS1841;
            if (censusYear == 1881 && Countries.IsEnglandWales(censusCountry))
                CensusDate = CensusDate.EWCENSUS1881;
            if (censusYear == 1881 && censusCountry == Countries.SCOTLAND)
                CensusDate = CensusDate.SCOTCENSUS1881;
            if (censusYear == 1881 && censusCountry == Countries.CANADA)
                CensusDate = CensusDate.CANADACENSUS1881;
            //if(censusYear == 1911 && censusCountry == Countries.IRELAND)
            //    CensusDate = CensusDate.IRELANDCENSUS1911;
            if (censusYear == 1911 && Countries.IsEnglandWales(censusCountry))
                CensusDate = CensusDate.EWCENSUS1911;
            if (censusYear == 1880 && censusCountry == Countries.UNITED_STATES)
                CensusDate = CensusDate.USCENSUS1880;
            if (censusYear ==1940 && censusCountry == Countries.UNITED_STATES)
                CensusDate = CensusDate.USCENSUS1940;

            int ptr = Name.IndexOf(",", StringComparison.Ordinal);
            if (ptr > 0)
            {
                string forenames = Name.Substring(ptr + 2);
                string surname = Name.Substring(0, ptr);
                ForenamesMetaphone = new DoubleMetaphone(forenames).PrimaryKey;
                SurnameMetaphone = new DoubleMetaphone(surname).PrimaryKey;
            }
            else
            {
                ForenamesMetaphone = string.Empty;
                SurnameMetaphone = string.Empty;
            }
        }

        public LostCousin(string name, string birthYear, string reference, string census, bool ftanalyzer)
        {
            Name = name;
            int.TryParse(birthYear, out int result);
            BirthYear = result;
            Reference = reference;
            FTAnalyzerFact = ftanalyzer;
            if(census.StartsWith("England"))
            {
                if (census.EndsWith("1841")) CensusDate = CensusDate.EWCENSUS1841;
                if (census.EndsWith("1881")) CensusDate = CensusDate.EWCENSUS1881;
                if (census.EndsWith("1911")) CensusDate = CensusDate.EWCENSUS1911;
            }
            if (census.StartsWith("Scotland") && census.EndsWith("1881")) CensusDate = CensusDate.SCOTCENSUS1881;
            if (census.StartsWith("Canada") && census.EndsWith("1881")) CensusDate = CensusDate.SCOTCENSUS1881;
            if (census.StartsWith("Ireland") && census.EndsWith("1881")) CensusDate = CensusDate.SCOTCENSUS1881;
            if (census.StartsWith("United States") && census.EndsWith("1880")) CensusDate = CensusDate.USCENSUS1880;
            if (census.StartsWith("United States") && census.EndsWith("1940")) CensusDate = CensusDate.USCENSUS1940;
        }

        string FixReference(string reference)
        {
            string output = reference;
            if (output.Length > 24)
                output = output.Substring(23); // strip the leading &census_code=XXXX&ref1=
            output = output.Replace("&ref2=", "/").Replace("&ref3=", "/").Replace("&ref4=", "/").Replace("&ref5=", "/");
            output = output.Replace("//", "/").Replace("//", "/").TrimEnd('/');
            return output;
        }

        public override string ToString()
        {
            return $"{Name} b.{BirthYear}, ref: {Reference} {CensusDate}";
        }

        public bool Equals(LostCousin other)
        {
            return CensusDate == other.CensusDate &&
                    (Name == other.Name || (ForenamesMetaphone == other.ForenamesMetaphone &&
                                            SurnameMetaphone == other.SurnameMetaphone)) &&
                    Math.Abs(BirthYear - other.BirthYear) < 2 &&
                    Reference == other.Reference;
        }
    }
}
