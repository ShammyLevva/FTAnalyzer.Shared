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
                int count = Facts.Count(x => x.Individual != null);
                count += Facts.Count(x => x.Family != null && x.Family.Husband != null);
                count += Facts.Count(x => x.Family != null && x.Family.Wife != null);
                return count;
            }
        }

        public void FixSourceID(int length)
        {
            try
            {
                if (!string.IsNullOrEmpty(SourceID))
                    SourceID = SourceID.Substring(0, 1) + SourceID.Substring(1).PadLeft(length, '0');
            }
            catch (Exception)
            { // don't error if SourceID is not of format Sxxxx
            }
        }

        public bool IsBirthCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().IndexOf(BIRTHCERT, StringComparison.Ordinal) >= 0;

        public bool IsDeathCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().IndexOf(DEATHCERT, StringComparison.Ordinal) >= 0;

        public bool IsMarriageCert() => SourceMedium.Contains("Official Document") &&
                   SourceTitle.ToUpper().IndexOf(MARRIAGECERT, StringComparison.Ordinal) >= 0;

        public bool IsCensusCert() => SourceMedium.Equals("Official Document") &&
                   SourceTitle.ToUpper().IndexOf(CENSUSCERT, StringComparison.Ordinal) >= 0;

        public override string ToString() => SourceTitle;

        public IComparer<IDisplaySource> GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "SourceID": return CompareComparableProperty<IDisplaySource>(f => f.SourceID, ascending);
                case "SourceTitle": return CompareComparableProperty<IDisplaySource>(f => f.SourceTitle, ascending);
                case "Publication": return CompareComparableProperty<IDisplaySource>(f => f.Publication, ascending);
                case "Author": return CompareComparableProperty<IDisplaySource>(f => f.Author, ascending);
                case "SourceText": return CompareComparableProperty<IDisplaySource>(f => f.SourceText, ascending);
                case "SourceMedium": return CompareComparableProperty<IDisplaySource>(f => f.SourceMedium, ascending);
                case "FactCount": return CompareComparableProperty<IDisplaySource>(f => f.FactCount, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareComparableProperty<T>(Func<IDisplaySource, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as IDisplaySource);
                var c2 = accessor(y as IDisplaySource);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}
