using System;
using System.Text.RegularExpressions;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public class Age : IComparable<Age>
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Age));

        public int MinAge { get; private set; }
        public int MaxAge { get; private set; }
        public FactDate CalculatedBirthDate { get; private set; }
        public string GEDCOM_Age { get; private set; }

        readonly string age;

        public static Age BIRTH = new Age();

        Age()
        {
            MinAge = 0;
            MaxAge = 0;
            age = "0";
            GEDCOM_Age = string.Empty;
            CalculatedBirthDate = FactDate.UNKNOWN_DATE;
        }

        public Age(Individual ind, FactDate when)
            : this()
        {
            if (when.IsAfter(ind.DeathDate))
            {
                when = ind.DeathDate;
            }

            log.Debug($"Calculating Age for {ind.Name} on {when}");
            log.Debug($"Min age: birth enddate: {ind.BirthDate.EndDate} to startdate: {when.StartDate}");
            log.Debug($"Max age: birth startdate: {ind.BirthDate.StartDate} to enddate: {when.EndDate}");

            MinAge = GetAge(ind.BirthDate.EndDate, when.StartDate);
            MaxAge = GetAge(ind.BirthDate.StartDate, when.EndDate);

            log.Debug($"Calculated minage: {MinAge} calculated maxage: {MaxAge}");
            if (MinAge == FactDate.MINYEARS)
            {
                age = (MaxAge == FactDate.MAXYEARS) ? "Unknown" :
                    MaxAge == 0 ? "< 1" : $"<= {MaxAge}";
            }
            else if (MaxAge < FactDate.MAXYEARS)
            {
                age = MinAge == MaxAge ? $"{MinAge}" : $"{MinAge} to ${MaxAge}";
            }
            else
            {
                // if age over maximum return maximum
                age = $">= {MinAge}";
            }
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

                var year = matcher.Groups["year"].ToString().TrimEnd('y');
                var month = matcher.Groups["month"].ToString().TrimEnd('m');
                var day = matcher.Groups["day"].ToString().TrimEnd('d');

                var startDate = when.StartDate;
                var endDate = when.EndDate;
                if (int.TryParse(year, out int yearno))
                {
                    if (startDate != FactDate.MINDATE && startDate.Year > yearno + 1)
                    {
                        startDate = startDate.TryAddYears(-yearno);
                    }
                    endDate = endDate.TryAddYears(-yearno);
                }
                if (int.TryParse(month, out int monthno))
                {
                    if (startDate != FactDate.MINDATE && startDate.Year > 1)
                    {
                        startDate = startDate.AddMonths(-monthno);
                    }
                    endDate = endDate.AddMonths(-monthno);
                }
                if (int.TryParse(day, out int dayno))
                {  // -dayno + 1 as date will be at time 00:00 and subtraction is one day too much.
                    if (startDate != FactDate.MINDATE && startDate.Year > 1)
                    {
                        startDate = startDate.AddDays(-dayno);
                    }
                    endDate = endDate.AddDays(-dayno);
                }
                CalculatedBirthDate = new FactDate(startDate, endDate);
            }
        }

        int GetAge(DateTime birthDate, DateTime laterDate)
        {
            var result = laterDate.Year - birthDate.Year;
            if (laterDate.DayOfYear <= birthDate.DayOfYear)
            {
                result++;
            }

            result = Math.Max(0, Math.Min(result, FactDate.MAXYEARS));

            return result;
        }

        public FactDate GetBirthDate(FactDate when)
        {
            if (CalculatedBirthDate.IsKnown)
            {
                return CalculatedBirthDate;
            }

            var startDate = when.StartDate.TryAddYears(-MaxAge);
            var endDate = when.EndDate.TryAddYears(-MinAge);
            return new FactDate(startDate, endDate);
        }

        public override string ToString() => age;

        public int CompareTo(Age that) =>
            MinAge == that.MinAge ? MaxAge - that.MaxAge :
                MinAge - that.MinAge;

        public override bool Equals(object obj) =>
            (obj is Age that) && MaxAge == that.MaxAge && MinAge == that.MinAge;

        public override int GetHashCode() => base.GetHashCode();
    }
}
