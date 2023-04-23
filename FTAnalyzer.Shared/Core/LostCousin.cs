﻿using System;
using System.Web;

namespace FTAnalyzer
{
    public class LostCousin : IEquatable<LostCousin>, IDisplayLostCousin
    {
        public string Name { get; }
        public int BirthYear { get; }
        public string Reference { get; }
        public CensusDate CensusDate { get; }
        public Uri WebLink { get; }
        public bool FTAnalyzerFact { get; }
        string SurnameMetaphone { get; set; }
        string ForenameMetaphone { get; set; }

        public enum Status { Good = 1, FuzzyNameAge = 2, NotPrecise = 3, Bad = 4}

        public LostCousin(string name, int birthYear, string reference, int censusYear, string censusCountry, bool ftanalyzer)
        {
            Name = name;
            BirthYear = birthYear;
            Reference = FixReference(reference ?? string.Empty);
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
            if (censusYear == 1940 && censusCountry == Countries.UNITED_STATES)
                CensusDate = CensusDate.USCENSUS1940;
            SetMetaphones();
        }

        public LostCousin(string name, string birthYear, string reference, string census, string weblink, bool ftanalyzer)
        { // Lost from website constructor
            Name = name;
            SetMetaphones();
            _ = int.TryParse(birthYear, out int result);
            BirthYear = result;
            Reference = reference;
            int ptr = weblink is null ? -1 : weblink.IndexOf("&p=", StringComparison.Ordinal);
            WebLink = ptr == -1 || weblink.Length <= ptr+3 ? null : new Uri(HttpUtility.UrlDecode(weblink[(ptr + 3)..]));
            FTAnalyzerFact = ftanalyzer;
            census ??= string.Empty;
            if (census.StartsWith("England", StringComparison.Ordinal))
            {
                if (census.EndsWith("1841", StringComparison.Ordinal)) CensusDate = CensusDate.EWCENSUS1841;
                if (census.EndsWith("1881", StringComparison.Ordinal)) CensusDate = CensusDate.EWCENSUS1881;
                if (census.EndsWith("1911", StringComparison.Ordinal)) CensusDate = CensusDate.EWCENSUS1911;
            }
            else
            {
                if (census.StartsWith("Scotland", StringComparison.Ordinal) && census.EndsWith("1881", StringComparison.Ordinal)) CensusDate = CensusDate.SCOTCENSUS1881;
                if (census.StartsWith("Canada", StringComparison.Ordinal) && census.EndsWith("1881", StringComparison.Ordinal)) CensusDate = CensusDate.SCOTCENSUS1881;
                if (census.StartsWith("Ireland", StringComparison.Ordinal) && census.EndsWith("1881", StringComparison.Ordinal)) CensusDate = CensusDate.SCOTCENSUS1881;
                if (census.StartsWith("United States", StringComparison.Ordinal) && census.EndsWith("1880", StringComparison.Ordinal)) CensusDate = CensusDate.USCENSUS1880;
                if (census.StartsWith("United States", StringComparison.Ordinal) && census.EndsWith("1940", StringComparison.Ordinal)) CensusDate = CensusDate.USCENSUS1940;
            }
        }

        void SetMetaphones()
        {
            int ptr = Name.IndexOf(",", StringComparison.Ordinal);
            if (ptr > 0)
            {
                string forenames = ptr + 2 < Name.Length ? Name[(ptr + 2)..] : string.Empty;
                string surname = Name[..ptr];
                int pos = forenames.IndexOf(" ", StringComparison.Ordinal);
                string forename = forenames is null ? string.Empty : (pos > 0 ? forenames[..pos] : forenames);
                ForenameMetaphone = new DoubleMetaphone(forename).PrimaryKey;
                SurnameMetaphone = new DoubleMetaphone(surname).PrimaryKey;
            }
            else
            {
                ForenameMetaphone = string.Empty;
                SurnameMetaphone = string.Empty;
            }
        }

        public bool Verified => false;

        static string FixReference(string reference)
        {
            string output = reference;
            if (output.Length > 24)
                output = output[23..]; // strip the leading &census_code=XXXX&ref1=
            output = output.Replace("&ref2=", "/").Replace("&ref3=", "/").Replace("&ref4=", "/").Replace("&ref5=", "/");
            output = output.Replace("//", "/").Replace("//", "/").TrimEnd('/');
            return output;
        }

        public override string ToString() => $"{Name} b.{BirthYear}, ref: {Reference} {CensusDate}";

        public bool Equals(LostCousin? other)
        {
            if (other is null) return false;
            if(CensusDate != other.CensusDate || Reference != other.Reference || Math.Abs(BirthYear - other.BirthYear) >= 5)
                return false;
            if (Name == other.Name)
                return true;
            if (ForenameMetaphone == other.ForenameMetaphone && SurnameMetaphone == other.SurnameMetaphone)
                return true;
            return false;
        }

        public override bool Equals(object? obj) => Equals(obj as LostCousin);

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
