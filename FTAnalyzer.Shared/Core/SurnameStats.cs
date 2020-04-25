using System.Collections;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class SurnameStats
    {
        public string Surname { get; private set; }
        public int Individuals { get; set; }
        public int Families { get; set; }
        public int Marriages { get; set; }
        public string GOONSpage { get; set; }

        public SurnameStats(string name)
        {
            Surname = name;
            Individuals = 0;
            Families = 0;
            Marriages = 0;
            GOONSpage = string.Empty;
        }
    }

    public class SurnameStatsComparer : IEqualityComparer<SurnameStats>
    { 
        public bool Equals(SurnameStats a, SurnameStats b)
        {
            if (a == null || b == null)
                return false;
            return a.Surname.ToUpper() == b.Surname.ToUpper() &&
                    a.Individuals == b.Individuals &&
                    a.Families == b.Families &&
                    a.Marriages == b.Marriages;
        }

        public int GetHashCode(SurnameStats obj) => base.GetHashCode();
    }
}
