using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace FTAnalyzer
{
    public class FactSource : IDisplaySource
    {
        private static readonly string BIRTHCERT = "BIRTH";
        private static readonly string DEATHCERT = "DEATH";
        private static readonly string MARRIAGECERT = "MARRIAGE";
        private static readonly string CENSUSCERT = "CENSUS";

        public string SourceID { get; private set; }
        public string SourceTitle { get; private set; }
        public string Publication { get; private set; }
        public string Author { get; private set; }
        public string SourceText { get; private set; }
        public string SourceMedium { get; private set; }
        public List<Fact> Facts { get; private set; }

        public FactSource(XmlNode node)
        {
            SourceID = node.Attributes["ID"].Value;
            SourceTitle = FamilyTree.GetText(node, "TITL", true);
            Publication = FamilyTree.GetText(node, "PUBL", true);
            Author = FamilyTree.GetText(node, "AUTH", true);
            SourceText = FamilyTree.GetText(node, "TEXT", true);
            SourceMedium = FamilyTree.GetText(node, "REPO/CALN/MEDI", true);
            if (SourceMedium.Length == 0)
                SourceMedium = FamilyTree.GetText(node, "NOTE/CONC", true);
            Facts = new List<Fact>();
        }

        public void AddFact(Fact f)
        {
            if (!Facts.Contains(f))
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
    }
}
