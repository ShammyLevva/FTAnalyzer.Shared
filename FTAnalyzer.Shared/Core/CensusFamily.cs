using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FTAnalyzer
{
    public class CensusFamily : Family, IDisplayChildrenStatus
    {
        public CensusDate CensusDate { get; private set; }
        public FactLocation BestLocation { get; private set; }
        public new CensusIndividual Husband { get; private set; }
        public new CensusIndividual Wife { get; private set; }
        public new List<CensusIndividual> Children { get; private set; }

        List<CensusIndividual> FamilyChildren { get; set; }
        Family BaseFamily { get; set; }

        public CensusFamily(Family f, CensusDate censusDate)
            : base(f)
        {
            BaseFamily = f;
            CensusDate = censusDate;
            BestLocation = null;
            int position = 1;

            if (f.Wife != null)
                Wife = new CensusIndividual(position++, f.Wife, this, CensusIndividual.WIFE);

            if (f.Husband != null)
                Husband = new CensusIndividual(position++, f.Husband, this, CensusIndividual.HUSBAND);
            Children = new List<CensusIndividual>();
            foreach (Individual child in f.Children)
            {
                CensusIndividual toAdd = new CensusIndividual(position++, child, this, CensusIndividual.CHILD);
                Children.Add(toAdd);
            }
            FamilyChildren = new List<CensusIndividual>(Children); // Family children is all children alive or dead at census date
        }

        public new IEnumerable<CensusIndividual> Members
        {
            get
            {
                if (Husband != null) { yield return Husband; }
                if (Wife != null) { yield return Wife; }
                if (Children != null && Children.Count > 0)
                    foreach (CensusIndividual child in Children) { yield return child; }
            }
        }

        public bool Process(CensusDate censusDate, bool censusDone, bool checkCensus)
        {
            bool result = false;
            var facts = new List<Fact>();
            CensusDate = censusDate;
            if (IsValidFamily()) // valid family is one where any member is alive on census
            {
                if (censusDate == CensusDate.UKCENSUS1841)
                    Console.Write("break");
                if (IsValidIndividual(Wife, censusDone, true, checkCensus))
                {
                    result = true;
                    facts.AddRange(Wife.PersonalFacts);
                }
                else
                    Wife = null;
                // overwrite bestLocation by husbands as most commonly the family
                // end up at husbands location after marriage
                if (IsValidIndividual(Husband, censusDone, true, checkCensus))
                {
                    result = true;
                    facts.AddRange(Husband.PersonalFacts);
                }
                else
                    Husband = null;
                // update bestLocation by marriage date as Husband and Wife 
                // locations are often birth locations
                var marriage = GetPreferredFact(Fact.MARRIAGE);
                if (marriage != null)
                    facts.Add(marriage);

                var censusChildren = new List<CensusIndividual>();
                // sort children oldest first
                Children.Sort(new CensusAgeComparer());
                foreach (var child in Children)
                {
                    // set location to childs birth location
                    // this will end up setting birth location of last child 
                    // as long as the location is at least Parish level
                    if (IsValidIndividual(child, censusDone, false, checkCensus))
                    {
                        result = true;
                        censusChildren.Add(child);
                        facts.AddRange(child.PersonalFacts);
                    }
                }
                Children = censusChildren;
                BestLocation = FactLocation.BestLocation(facts, censusDate);
            }
            return result;
        }

        bool IsValidIndividual(CensusIndividual indiv, bool censusDone, bool parentCheck, bool checkCensus)
        {
            if (indiv is null)
                return false;

            DateTime birth = indiv.BirthDate.StartDate;
            DateTime death = indiv.DeathDate.EndDate;
            FactLocation bestLocation = indiv.BestLocation(CensusDate);

            if (birth <= CensusDate.StartDate && death >= CensusDate.StartDate)
            {
                if ((checkCensus && indiv.IsCensusDone(CensusDate) == censusDone && !indiv.OutOfCountry(CensusDate)) || !checkCensus)
                {
                    return parentCheck || !indiv.IsMarried(CensusDate);
                }
                return false;
            }
            return false;
        }

        bool IsValidFamily()
        {
            Individual eldestChild = Children.OrderBy(x => x.BirthDate).FirstOrDefault();
            if (eldestChild != null && eldestChild.BirthDate.IsBefore(CensusDate))
                return true;

            if (FamilyType == SOLOINDIVIDUAL || FamilyType == PRE_MARRIAGE || FamilyType == UNKNOWN)
                return true; // allow solo individual families to be processed

            // valid family if both parent are 16+ or sole parent >=16
            return (Husband != null && Husband.BirthDate.IsBefore(CensusDate)) || (Wife != null && Wife.BirthDate.IsBefore(CensusDate));
        }

        public new string Surname
        {
            get
            {
                if (Husband != null) { return Husband.SurnameAtDate(CensusDate); }
                if (Wife != null) { return Wife.SurnameAtDate(CensusDate); }

                Individual child = Children.FirstOrDefault();
                return child != null ? child.SurnameAtDate(CensusDate) : Individual.UNKNOWN_NAME;
            }
        }

        public int ChildrenAlive => FamilyChildren.Count(x => x.IsAlive(CensusDate));

        public int ChildrenDead => FamilyChildren.Count(x => !x.IsAlive(CensusDate) && x.BirthDate.IsBefore(CensusDate));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Debug.WriteLine(System.String)")]
        public int ChildrenTotal
        {
            get
            {
                int total = ChildrenAlive + ChildrenDead;
                if (total == 0)
                    Debug.WriteLine("hmmm zero?");
                return total;
            }
        }

        string IDisplayChildrenStatus.Husband =>
                BaseFamily.Husband is null ? string.Empty : $"{BaseFamily.Husband.Name} (b. {BaseFamily.Husband.BirthDate})";

        string IDisplayChildrenStatus.Wife =>
                BaseFamily.Wife is null ? string.Empty : $"{BaseFamily.Wife.Name} (b.{BaseFamily.Wife.BirthDate})";
    }
}