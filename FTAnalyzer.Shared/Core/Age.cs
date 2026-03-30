using FTAnalyzer.Utilities;
using System.Text.RegularExpressions;

namespace FTAnalyzer
{
    public partial class Age : IComparable<Age>, IComparable
    {
        public int MinAge { get; private set; }
        public int MaxAge { get; private set; }
        public FactDate CalculatedBirthDate { get; private set; }
        public string GEDCOM_Age { get; private set; }
        readonly string _age;
        public static readonly Age BIRTH = new();

        Age()
        {
            MinAge = 0;
            MaxAge = 0;
            _age = "0";
            GEDCOM_Age = string.Empty;
            CalculatedBirthDate = FactDate.UNKNOWN_DATE;
        }
        public Age(Individual ind, FactDate when)
            : this(ind.BirthDate, when.IsAfter(ind.DeathDate) ? ind.DeathDate : when)
        { }
        public Age(FactDate birthdate, FactDate deathdate)
            : this()
        {
            MinAge = GetAge(birthdate.EndDate, deathdate.StartDate);
            MaxAge = GetAge(birthdate.StartDate, deathdate.EndDate);
            if (MinAge == FactDate.MINYEARS)
                _age = (MaxAge == FactDate.MAXYEARS) ? "Unknown" : MaxAge == 0 ? "< 1" : $"<= {MaxAge}";
            else if (MaxAge < FactDate.MAXYEARS)
                _age = MinAge == MaxAge ? $"{MinAge}" : $"{MinAge} to {MaxAge}";
            else
                _age = $">= {MinAge}"; // if age over maximum return maximum
        }

        public Age(string gedcomAge, FactDate when)
            : this()
        {
            // parse ages from gedcom
            Match matcher = RegexPatterns.AgeRegex().Match(gedcomAge);
            if (matcher.Success)
            {
                GEDCOM_Age = gedcomAge;

                string year = matcher.Groups["year"].ToString().TrimEnd('y');
                string month = matcher.Groups["month"].ToString().TrimEnd('m');
                string day = matcher.Groups["day"].ToString().TrimEnd('d');

                DateTime startDate = when.StartDate;
                DateTime endDate = when.EndDate;
                if (int.TryParse(year, out int yearno))
                {
                    if (startDate != FactDate.MINDATE && startDate.Year > yearno + 1)
                        startDate = startDate.TryAddYears(-yearno);
                    endDate = endDate.TryAddYears(-yearno);
                }
                if (int.TryParse(month, out int monthno))
                {
                    if (startDate != FactDate.MINDATE && startDate.Year > 1)
                        startDate = startDate.AddMonths(-monthno);
                    endDate = endDate.AddMonths(-monthno);
                }
                if (int.TryParse(day, out int dayno))
                {  // -dayno + 1 as date will be at time 00:00 and subtraction is one day too much.
                    if (startDate != FactDate.MINDATE && startDate.Year > 1)
                        startDate = startDate.AddDays(-dayno);
                    endDate = endDate.AddDays(-dayno);
                }
                CalculatedBirthDate = new FactDate(startDate, endDate);
            }
        }

        static int GetAge(DateTime birthDate, DateTime laterDate)
        {
            int age = laterDate.Year - birthDate.Year;
            if (laterDate.DayOfYear < birthDate.DayOfYear)
                age--;
            age = Math.Max(0, Math.Min(age, FactDate.MAXYEARS));
            return age;
        }

        public FactDate GetBirthDate(FactDate when)
        {
            if (CalculatedBirthDate.IsKnown)
                return CalculatedBirthDate;
            DateTime startDate = when.StartDate.TryAddYears(-MaxAge);
            DateTime endDate = when.EndDate.TryAddYears(-MinAge);
            return new FactDate(startDate, endDate);
        }

        public override string ToString() => _age;

        public int CompareTo(Age? that)
        {
            // 1. Check for null: An instance is always greater than null
            if (that is null) return 1;

            // 2. Optimization: Check if they are the exact same instance
            if (ReferenceEquals(this, that)) return 0;

            // 3. Your comparison logic
            if (MinAge != that.MinAge)
                return MinAge - that.MinAge;

            int result = MinAge.CompareTo(that.MinAge);
            if (result == 0)
                result = MaxAge.CompareTo(that.MaxAge);
            return result;
        }

        public int CompareTo(object? that) => CompareTo(that as Age);

        public override bool Equals(object? obj) =>
            (obj is Age that) && MaxAge == that.MaxAge && MinAge == that.MinAge;

        public override int GetHashCode() => base.GetHashCode();
    }
}
