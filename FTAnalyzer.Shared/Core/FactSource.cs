using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace FTAnalyzer
{
    public class FactSource : IDisplaySource
    {
        const string BIRTHCERT = "BIRTH";
        const string DEATHCERT = "DEATH";
        const string MARRIAGECERT = "MARRIAGE";
        const string CENSUSCERT = "CENSUS";

        public string SourceID { get; private set; }
        public string SourceTitle { get; private set; }
        public string Publication { get; private set; }
        public string Author { get; private set; }
        public string SourceText { get; private set; }
        public string SourceMedium { get; private set; }
        public List<Fact> Facts { get; private set; }

        public FactSource(XmlNode node)
        {
            bool noteRead = false;
            SourceID = node.Attributes["ID"].Value;
            SourceTitle = FamilyTree.GetText(node, "TITL", true);
            Publication = FamilyTree.GetText(node, "PUBL", true);
            Author = FamilyTree.GetText(node, "AUTH", true);
            SourceText = FamilyTree.GetText(node, "TEXT", true);
            if (string.IsNullOrEmpty(SourceText))
            {
                SourceText = FamilyTree.GetText(node, "NOTE", true);
                noteRead = true;
            }
            SourceMedium = FamilyTree.GetText(node, "REPO/CALN/MEDI", true);
            if (!noteRead && SourceMedium.Length == 0)
                SourceMedium = FamilyTree.GetText(node, "NOTE/CONC", true);
            Facts = new List<Fact>();
        }

        public void AddFact(Fact f)
        {
            if (!Facts.ContainsFact(f))
                Facts.Add(f);
        }

        public int FactCount
        {
            get
            {
                int count = Facts.Count(x => x.Individual is not null);
                count += Facts.Count(x => x.Family is not null && x.Family.Husband is not null);
                count += Facts.Count(x => x.Family is not null && x.Family.Wife is not null);
                return count;
            }
        }

        public void FixSourceID(int length)
        {
            try
            {
                if (!string.IsNullOrEmpty(SourceID))
                    SourceID = SourceID[..1] + SourceID[1..].PadLeft(length, '0');
            }
            catch (Exception)
            { // don't error if SourceID is not of format Sxxxx
            }
        }

        public bool IsBirthCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().Contains(BIRTHCERT);

        public bool IsDeathCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().Contains(DEATHCERT);

        public bool IsMarriageCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().Contains(MARRIAGECERT);

        public bool IsCensusCert() => SourceMedium.Equals("Official Document") &&
                   SourceTitle.ToUpper().Contains(CENSUSCERT);

        public override string ToString() => SourceTitle;

        public IComparer<IDisplaySource> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "SourceID" => CompareComparableProperty<IDisplaySource>(f => f.SourceID, ascending),
                "SourceTitle" => CompareComparableProperty<IDisplaySource>(f => f.SourceTitle, ascending),
                "Publication" => CompareComparableProperty<IDisplaySource>(f => f.Publication, ascending),
                "Author" => CompareComparableProperty<IDisplaySource>(f => f.Author, ascending),
                "SourceText" => CompareComparableProperty<IDisplaySource>(f => f.SourceText, ascending),
                "SourceMedium" => CompareComparableProperty<IDisplaySource>(f => f.SourceMedium, ascending),
                "FactCount" => CompareComparableProperty<IDisplaySource>(f => f.FactCount, ascending),
                _ => CompareComparableProperty<IDisplaySource>(f => f.SourceID, ascending),
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplaySource, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                if (x is not IDisplaySource srcX)
                    return ascending ? 1 : -1;
                if (y is not IDisplaySource srcY)
                    return ascending ? 1 : -1;
                var c1 = accessor(srcX);
                var c2 = accessor(srcY);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}
