using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class FactComparer : IEqualityComparer<Fact>
    {
        public bool Equals(Fact x, Fact y) => x.FactType == y.FactType && x.FactDate == y.FactDate && x.Location == y.Location;

        public int GetHashCode(Fact obj) => IntDate(obj.FactDate.StartDate) * 10 + IntDate(obj.FactDate.EndDate);

        int IntDate(DateTime date) => (date.Year * 100 + date.Month) * 100 + date.Day;
    }
}
