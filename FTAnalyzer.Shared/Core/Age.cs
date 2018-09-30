using System;
using System.Text.RegularExpressions;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public class Age : IComparable<Age>
    {
        static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Age));

        public int MinAge { get; private set; }
        public int MaxAge { get; private set; }
        public FactDate CalculatedBirthDate { get; private set; }
        public string GEDCOM_Age { get; private set; }
        readonly string _age;
        public static Age BIRTH = new Age();

        Age()
        {
            MinAge = 0;
            MaxAge = 0;
            _age = "0";
            GEDCOM_Age = string.Empty;
            CalculatedBirthDate = FactDate.UNKNOWN_DATE;
        }

        public Age(Individual ind, FactDate when)
            : this()
        {
            if (when.IsAfter(ind.DeathDate))
                when = ind.DeathDate;

            Logger.Debug($"Calculating Age for {ind.Name} on {when}");
            Logger.Debug($"Min age: birth enddate: {ind.BirthDate.EndDate} to startdate: {when.StartDate}");
            Logger.Debug($"Max age: birth startdate: {ind.BirthDate.StartDate} to enddate: {when.EndDate}");

            MinAge = GetAge(ind.BirthDate.EndDate, when.StartDate);
            MaxAge = GetAge(ind.BirthDate.StartDate, when.EndDate);

            Logger.Debug($"Calculated minage: {MinAge} calculated maxage: {MaxAge}");
            if (MinAge == FactDate.MINYEARS)
                _age = (MaxAge == FactDate.MAXYEARS) ? "Unknown" : MaxAge == 0 ? "< 1" : $"<= {MaxAge}";
            else if (MaxAge < FactDate.MAXYEARS)
                _age = MinAge == MaxAge ? $"{MinAge}" : $"{MinAge} to {MaxAge}";
            else
                _age = $">= {MinAge}"; // if age over maximum return maximum
        }

        static readonly string pattern = @"^(?<year>\d{1,3}y)? ?(?<month>\d{1,2}m)? ?(?<day>\d{1,2}d)?$";
        static Regex ydm = new Regex(pattern, RegexOptions.Compiled);

        public Age(string gedcomAge, FactDate when)
            : this()
        {
            // parse ages from gedcom
            var matcher = ydm.Match(gedcomAge);
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

        int GetAge(DateTime birthDate, DateTime laterDate)
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

        public int CompareTo(Age that) =>
            MinAge == that.MinAge ? MaxAge - that.MaxAge : MinAge - that.MinAge;

        public override bool Equals(object obj) =>
            (obj is Age that) && MaxAge == that.MaxAge && MinAge == that.MinAge;

        public override int GetHashCode() => base.GetHashCode();
    }
}
