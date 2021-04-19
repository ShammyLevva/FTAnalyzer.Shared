using FTAnalyzer.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class SurnameStats : IDisplaySurnames
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

        public int CompareTo(IDisplaySurnames other)
        {
            throw new System.NotImplementedException();
        }

        public IComparer<IDisplaySurnames> GetComparer(string columnName, bool ascending)
        {
            throw new System.NotImplementedException();
        }
    }

    public class SurnameStatsComparer : IEqualityComparer<IDisplaySurnames>
    {
        public bool Equals(IDisplaySurnames a, IDisplaySurnames b)
        {
            return a.Surname.ToUpper() == b.Surname.ToUpper() &&
                    a.Individuals == b.Individuals &&
                    a.Families == b.Families &&
                    a.Marriages == b.Marriages;
        }

        public int GetHashCode(IDisplaySurnames obj) => base.GetHashCode();
    }
}
