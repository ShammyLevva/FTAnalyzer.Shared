using FTAnalyzer.Exports;
using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FTAnalyzer
{
    public class Family : IDisplayFamily, IJsonFamily
    {
        public const string UNKNOWN = "Unknown", SOLOINDIVIDUAL = "Solo", PRE_MARRIAGE = "Pre-Marriage";
        public const string SINGLE = "Single", MARRIED = "Married", UNMARRIED = "Unmarried";

        public string FamilyID { get; private set; }
        public IList<Fact> Facts { get; private set; }
        public List<Individual> Children { get; internal set; }
        public Individual Husband { get; internal set; }
        public Individual Wife { get; internal set; }
        public int ExpectedTotal { get; internal set; }
        public int ExpectedAlive { get; internal set; }
        public int ExpectedDead { get; internal set; }
        public string FamilyType { get; internal set; }

        readonly IDictionary<string, Fact> _preferredFacts;
        readonly FamilyTree ft = FamilyTree.Instance;

        Family(string familyID)
        {
            FamilyID = familyID;
            Facts = new List<Fact>();
            Children = new List<Individual>();

            _preferredFacts = new Dictionary<string, Fact>();

            ExpectedTotal = 0;
            ExpectedAlive = 0;
            ExpectedDead = 0;

            FamilyType = familyID.StartsWith("SF", StringComparison.Ordinal) ? SOLOINDIVIDUAL
                       : familyID.StartsWith("PM", StringComparison.Ordinal) ? PRE_MARRIAGE
                       : UNKNOWN;
        }

        public Family() : this(string.Empty) { }

        public Family(XmlNode node, IProgress<string> outputText)
            : this(string.Empty)
        {
            if (node != null)
            {
                XmlNode eHusband = node.SelectSingleNode("HUSB");
                XmlNode eWife = node.SelectSingleNode("WIFE");
                FamilyID = node.Attributes["ID"].Value;
                string husbandID = eHusband?.Attributes["REF"]?.Value;
                string wifeID = eWife?.Attributes["REF"]?.Value;
                Husband = ft.GetIndividual(husbandID);
                Wife = ft.GetIndividual(wifeID);
                if (Husband != null && Wife != null)
                    Wife.MarriedName = Husband.Surname;
                if (Husband != null)
                    Husband.FamiliesAsSpouse.Add(this);
                if (Wife != null)
                    Wife.FamiliesAsSpouse.Add(this);

                // now iterate through child elements of eChildren
                // finding all individuals
                XmlNodeList list = node.SelectNodes("CHIL");
                foreach (XmlNode n in list)
                {
                    if (n.Attributes["REF"] != null)
                    {
                        Individual child = ft.GetIndividual(n.Attributes["REF"].Value);
                        if (child != null)
                        {
                            XmlNode fatherNode = n.SelectSingleNode("_FREL");
                            XmlNode motherNode = n.SelectSingleNode("_MREL");
                            var father = ParentalRelationship.GetRelationshipType(fatherNode);
                            var mother = ParentalRelationship.GetRelationshipType(motherNode);
                            Children.Add(child);
                            var parent = new ParentalRelationship(this, father, mother);
                            child.FamiliesAsChild.Add(parent);
                            AddParentAndChildrenFacts(child, Husband, father);
                            AddParentAndChildrenFacts(child, Wife, mother);
                        }
                        else
                            outputText.Report($"Child not found in family: {FamilyRef}\n");
                    }
                    else
                        outputText.Report($"Child without a reference found in family: {FamilyRef}\n");
                }

                AddFacts(node, Fact.ANNULMENT, outputText);
                AddFacts(node, Fact.DIVORCE, outputText);
                AddFacts(node, Fact.DIVORCE_FILED, outputText);
                AddFacts(node, Fact.ENGAGEMENT, outputText);
                AddFacts(node, Fact.MARRIAGE, outputText);
                AddFacts(node, Fact.MARRIAGE_BANN, outputText);
                AddFacts(node, Fact.MARR_CONTRACT, outputText);
                AddFacts(node, Fact.MARR_LICENSE, outputText);
                AddFacts(node, Fact.MARR_SETTLEMENT, outputText);
                AddFacts(node, Fact.SEPARATION, outputText);
                AddFacts(node, Fact.CENSUS, outputText);
                AddFacts(node, Fact.CUSTOM_EVENT, outputText);
                AddFacts(node, Fact.CUSTOM_FACT, outputText);
                AddFacts(node, Fact.REFERENCE, outputText);
                AddFacts(node, Fact.SEALED_TO_SPOUSE, outputText);
                AddFacts(node, Fact.UNKNOWN, outputText);

                //TODO: need to think about family facts having AGE tags in GEDCOM
                if (HasGoodChildrenStatus)
                    CheckChildrenStatusCounts();
                if (MarriageDate.IsKnown && !MarriageDate.Overlaps(FactDate.SAME_SEX_MARRIAGE)) // check for wrongly set gender only if pre-dates same sex marriages
                {
                    if (Husband != null && !Husband.IsMale)
                        Husband.QuestionGender(this, true);
                    if (Wife != null && Wife.IsMale)
                        Wife.QuestionGender(this, false);
                }
                Children.Sort(new BirthDateComparer());
            }
        }


        void CheckChildrenStatusCounts()
        {
            foreach (Fact f in GoodChildrenStatusFacts)
            {
                Match matcher = Fact.regexChildren1.Match(f.Comment);
                if (matcher.Success)
                    SetChildrenStatusCounts(matcher, 1, 2, 4);
                else
                {
                    matcher = Fact.regexChildren2.Match(f.Comment);
                    if (matcher.Success)
                        SetChildrenStatusCounts(matcher, 1, 3, 4);
                }
            }
        }

        void SetChildrenStatusCounts(Match matcher, int totalGroup, int aliveGroup, int deadGroup)
        {
            if (int.TryParse(matcher.Groups[totalGroup].Value, out int resultT))
                ExpectedTotal += resultT;
            if (int.TryParse(matcher.Groups[aliveGroup].Value, out int resultA))
                ExpectedAlive += resultA;
            if (int.TryParse(matcher.Groups[deadGroup].Value, out int resultD))
                ExpectedDead += resultD;
        }

        void AddParentAndChildrenFacts(Individual child, Individual parent, ParentalRelationship.ParentalRelationshipType prType)
        {
            if (parent != null)
            {
                string parentComment;
                string childrenComment;
                if (prType == ParentalRelationship.ParentalRelationshipType.UNKNOWN)
                {
                    parentComment = $"Child of {parent.IndividualID}: {parent.Name}";
                    childrenComment = $"Parent of {child.IndividualID}: {child.Name}";
                }
                else
                {
                    string titlecase = EnhancedTextInfo.ToTitleCase(prType.ToString().ToLower());
                    parentComment = $"{titlecase}  child of {parent.IndividualID}: {parent.Name}";
                    childrenComment = $"{titlecase} parent of {child.IndividualID}: {child.Name}";
                }

                Fact parentFact = new Fact(parent.IndividualID, Fact.PARENT, child.BirthDate, child.BirthLocation, parentComment, true, true);
                child.AddFact(parentFact);

                Fact childrenFact = new Fact(child.IndividualID, Fact.CHILDREN, child.BirthDate, child.BirthLocation, childrenComment, true, true);
                parent.AddFact(childrenFact);
            }
        }

        public Family(Individual ind, string familyID)
            : this(familyID)
        {
            if (ind.IsMale)
                Husband = ind;
            else
                Wife = ind;
        }

        internal Family(Family f)
        {
            FamilyID = f.FamilyID;
            Facts = new List<Fact>(f.Facts);
            Husband = f.Husband is null ? null : new Individual(f.Husband);
            Wife = f.Wife is null ? null : new Individual(f.Wife);
            Children = new List<Individual>(f.Children);
            _preferredFacts = new Dictionary<string, Fact>(f._preferredFacts);
            ExpectedTotal = f.ExpectedTotal;
            ExpectedAlive = f.ExpectedAlive;
            ExpectedDead = f.ExpectedDead;
            FamilyType = UNKNOWN;
        }

        void AddFacts(XmlNode node, string factType, IProgress<string> outputText)
        {
            XmlNodeList list = node.SelectNodes(factType);
            bool preferredFact = true;
            foreach (XmlNode n in list)
            {
                try
                {
                    Fact f = new Fact(n, this, preferredFact, outputText);
                    f.Location.FTAnalyzerCreated = false;
                    if (!f.Location.IsValidLatLong)
                        outputText.Report($"Found problem with Lat/Long for Location '{f.Location}' in facts for {FamilyID}: {FamilyName}");
                    if (string.IsNullOrEmpty(f.Comment) && Husband != null && Wife != null && f.IsMarriageFact)
                    {
                        string description = Fact.GetFactTypeDescription(factType);
                        f.Comment = $"{description} of {Husband.Name} and {Wife.Name}";
                    }
                    if (f.FactType != Fact.CENSUS)
                    {
                        Facts.Add(f);
                        if (!_preferredFacts.ContainsKey(f.FactType))
                            _preferredFacts.Add(f.FactType, f);
                    }
                    else
                    {
                        // Handle a census fact on a family.
                        if (GeneralSettings.Default.OnlyCensusParents)
                        {
                            if (Husband != null && Husband.IsAlive(f.FactDate))
                                Husband.AddFact(f);
                            if (Wife != null && Wife.IsAlive(f.FactDate))
                                Wife.AddFact(f);
                        }
                        else
                        {
                            // all members of the family who are alive get the census fact
                            foreach (Individual person in Members.Where(p => p.IsAlive(f.FactDate)))
                                person.AddFact(f);
                        }
                    }
                }
                catch (InvalidXMLFactException ex)
                {
                    outputText.Report($"Error with Family : {FamilyID}\n       Invalid fact : {ex.Message}");
                }
                catch (TextFactDateException te)
                {
                    if (te.Message == "UNMARRIED" || te.Message == "NEVER MARRIED" || te.Message == "NOT MARRIED")
                    {
                        Fact f = new Fact(string.Empty, Fact.UNMARRIED, FactDate.UNKNOWN_DATE, FactLocation.UNKNOWN_LOCATION, string.Empty, true, true);
                        Husband?.AddFact(f);
                        Wife?.AddFact(f);
                        Facts.Add(f);
                    }
                }
                preferredFact = false;
            }
        }

        public void FixFamilyID(int length)
        {
            if (string.IsNullOrEmpty(FamilyID))
            {
                FamilyType = SOLOINDIVIDUAL;
                FamilyID = ft.NextSoloFamily;
            }
            else
            {
                int prefixLength = FamilyType == SOLOINDIVIDUAL || FamilyType == PRE_MARRIAGE ? 2 : 1;
                if (FamilyID.Length >= prefixLength)
                {
                    string prefix = FamilyID.Substring(0, prefixLength);
                    string suffix = FamilyID.Substring(prefixLength);
                    FamilyID = prefix + suffix.PadLeft(length, '0');
                }
            }
        }

        /**
         * @return Returns the first fact of the given type.
         */
        public Fact GetPreferredFact(string factType) => _preferredFacts.ContainsKey(factType) ? _preferredFacts[factType] : null;

        /**
         * @return Returns the first fact of the given type.
         */
        public FactDate GetPreferredFactDate(string factType) => GetPreferredFact(factType)?.FactDate ?? FactDate.UNKNOWN_DATE;

        /**
         * @return Returns all facts of the given type.
         */
        public IEnumerable<Fact> GetFacts(string factType) => Facts.Where(f => f.FactType == factType);

        #region Properties

        public int FamilySize
        {
            get
            {
                int count = Children.Count;
                if (Husband != null)
                    count++;
                if (Wife != null)
                    count++;
                return count;
            }
        }

        public FactDate MarriageDate => GetPreferredFactDate(Fact.MARRIAGE);

        public string MarriageLocation => GetPreferredFact(Fact.MARRIAGE)?.Location?.ToString() ?? string.Empty;

        public string MaritalStatus
        {
            get
            {
                if (Husband is null || Wife is null || !MarriageDate.IsKnown)
                    return SINGLE;
                foreach (Fact f in Facts)
                    if (f.IsMarriageFact)
                        return MARRIED;
                return UNMARRIED;
            }
        }

        public string HusbandID => Husband?.IndividualID ?? string.Empty;

        public string WifeID => Wife?.IndividualID ?? string.Empty;

        public IEnumerable<Individual> Members
        {
            get
            {
                if (Husband != null) yield return Husband;
                if (Wife != null) yield return Wife;
                if (Children != null && Children.Count > 0)
                    foreach (Individual child in Children) yield return child;
            }
        }

        public IEnumerable<int> RelationTypes => Members.Select(m => m.RelationType);

        public bool HasUnknownRelations => RelationTypes.Contains(Individual.UNKNOWN);
        public bool HasLinkedRelations => RelationTypes.Contains(Individual.MARRIAGE) || RelationTypes.Contains(Individual.LINKED);

        public string FamilyName
        {
            get
            {
                string husbandsName = Husband?.Name ?? UNKNOWN;
                string wifesName = Wife?.Name ?? UNKNOWN;
                return $"{husbandsName} and {wifesName}";
            }
        }

        public string Surname
        {
            get
            {
                if (Husband != null) return Husband.Surname;
                return Wife is null ? string.Empty : Wife.Surname;
            }
        }

        public string Forenames => $"{HusbandForenames} {WifeForenames}".Trim();

        public string MarriageFilename => FamilyTree.ValidFilename($"{FamilyID} - Marriage of {FamilyName}.html");

        public string ChildrenFilename => FamilyTree.ValidFilename($"{FamilyID} - Children of {FamilyName}.html");

        public string FamilyRef
        {
            get
            {
                if (FamilyType.Equals(SOLOINDIVIDUAL))
                {
                    var name = Husband?.Name ?? Wife?.Name ?? string.Empty;
                    return $"Solo Family {FamilyID}: {name}";
                }
                return $"{FamilyID}: {FamilyName}";
            }
        }

        public Individual Spouse(Individual ind)
        {
            if (ind.Equals(Husband))
                return Wife;
            if (ind.Equals(Wife))
                return Husband;
            return null;
        }

        public bool ContainsSurname(string surname, bool ignoreCase) =>
                ignoreCase ? Members.Any(x => x.Surname.Equals(surname, StringComparison.OrdinalIgnoreCase)) :
                             Members.Any(x => x.Surname.Equals(surname));

        public bool On1911Census
        {
            get
            {
                if (Husband != null && Husband.IsCensusDone(CensusDate.UKCENSUS1911)) return true;
                if (Wife != null && Wife.IsCensusDone(CensusDate.UKCENSUS1911)) return true;
                return false;
            }
        }

        // only check shared facts for children status
        IEnumerable<Fact> GoodChildrenStatusFacts =>
                Facts.Where(f => f.FactType == Fact.CHILDREN1911 && f.FactErrorLevel == Fact.FactError.GOOD);

        public bool HasGoodChildrenStatus => GoodChildrenStatusFacts.Any();

        public bool HasAnyChildrenStatus => Facts.Any(f => f.FactType == Fact.CHILDREN1911);

        public Individual EldestChild => Children.Count == 0 ? null : Children[0];

        #endregion

        public void SetBudgieCode(Individual ind, int lenAhnentafel)
        {
            Individual spouse = ind.IsMale ? Wife : Husband;
            if (spouse != null && string.IsNullOrEmpty(spouse.BudgieCode))
                spouse.BudgieCode = ind.BudgieCode + "*s";
            int directChild = 0;
            if (ind.RelationType == Individual.DIRECT)
            {
                //first find which child is a direct
                foreach (var child in Children.OrderBy(c => c.BirthDate))
                {
                    directChild++;
                    if (child.RelationType == Individual.DIRECT)
                        break;
                }
            }
            if (directChild > 0)
            {
                int childcount = 0;
                foreach (Individual child in Children.OrderBy(c => c.BirthDate))
                {
                    childcount++;
                    if (string.IsNullOrEmpty(child.BudgieCode))
                    {
                        string prefix = (directChild < childcount) ? "+" : "-";
                        var code = Math.Abs(directChild - childcount);
                        BigInteger floor = ind.Ahnentafel / 2;
                        string ahnentafel = floor.ToString();
                        child.BudgieCode = ahnentafel.PadLeft(lenAhnentafel, '0') + prefix + code.ToString("D2");
                    }
                }
            }
            else
            {   // we have got here because we are not dealing with a direct nor a family that contains a direct child
                int childcount = 0;
                foreach (var child in Children.OrderBy(c => c.BirthDate))
                {
                    childcount++;
                    if (string.IsNullOrEmpty(child.BudgieCode))
                        child.BudgieCode = $"{ind.BudgieCode}.{childcount:D2}";
                }
            }
        }

        public void SetSpouseRelation(Individual ind, int relationType)
        {
            Individual spouse = ind.IsMale ? Wife : Husband;
            if (spouse != null)
                spouse.RelationType = relationType;
        }

        public void SetChildRelation(Queue<Individual> queue, int relationType)
        {
            foreach (Individual child in Children)
            {
                var previousType = child.RelationType;
                child.RelationType = relationType;
                if (child.RelationType != previousType)
                {
                    // add this changed individual to list 
                    // of relatives to update family of
                    queue.Enqueue(child);
                }
            }
        }

        public void SetChildrenCommonRelation(Individual parent, CommonAncestor commonAncestor)
        {
            foreach (var child in Children)
                if (child.CommonAncestor is null || child.CommonAncestor.Distance > commonAncestor.Distance + 1)
                    child.CommonAncestor = new CommonAncestor(commonAncestor.Ind, commonAncestor.Distance + 1, !child.IsNaturalChildOf(parent) || commonAncestor.Step);
        }

        #region IDisplayFamily Members

        string IDisplayFamily.Husband => Husband is null ? string.Empty : $"{Husband.Name} (b.{Husband.BirthDate})";
        string IJsonFamily.Husband => Husband is null ? string.Empty : $"{Husband.Name} (b.{Husband.BirthDate})";

        string IDisplayFamily.Wife => Wife is null ? string.Empty : $"{Wife.Name} (b. {Wife.BirthDate})";
        string IJsonFamily.Wife => Wife is null ? string.Empty : $"{Wife.Name} (b. {Wife.BirthDate})";

        public string Marriage => ToString();

        public string HusbandForenames => Husband is null ? string.Empty : Husband.Forenames;
        public string HusbandSurname => Husband is null ? string.Empty : Husband.Surname;
        public string WifeForenames => Wife is null ? string.Empty : Wife.Forenames;
        public string WifeSurname => Wife is null ? string.Empty : Wife.Surname;

        string IDisplayFamily.Children
        {
            get
            {
                var result = new StringBuilder();
                foreach (var c in Children)
                {
                    if (result.Length > 0)
                        result.Append(", ");
                    result.Append($"{c.IndividualID}: {c.Name} (b. {c.BirthDate})");
                }
                return result.ToString();
            }
        }

        public FactDate FamilyDate
        {
            get
            {
                // return "central" date of family - use marriage facts, Husband/Wife facts, children birth facts
                var dates = new List<FactDate>();

                foreach (var f in Facts.Where(f => f.FactDate.AverageDate.IsKnown))
                    dates.Add(f.FactDate.AverageDate);

                if (Husband != null)
                    foreach (var f in Husband.PersonalFacts.Where(f => f.FactDate.AverageDate.IsKnown))
                        dates.Add(f.FactDate.AverageDate);

                if (Wife != null)
                    foreach (var f in Wife.PersonalFacts.Where(f => f.FactDate.AverageDate.IsKnown))
                        dates.Add(f.FactDate.AverageDate);

                foreach (var c in Children.Where(c => c.BirthDate.AverageDate.IsKnown))
                    dates.Add(c.BirthDate.AverageDate);

                if (dates.Count == 0)
                    return FactDate.UNKNOWN_DATE;

                var averageTicks = 0L;
                foreach (var fd in dates)
                    averageTicks += fd.StartDate.Ticks / dates.Count;
                try
                {
                    var averageDate = new DateTime(averageTicks);
                    return new FactDate(averageDate, averageDate);
                }
                catch (ArgumentOutOfRangeException)
                {
                }

                return FactDate.UNKNOWN_DATE;
            }
        }

        public FactLocation Location => FactLocation.BestLocation(AllFamilyFacts, FamilyDate);

        #endregion

        public bool IsAtLocation(FactLocation loc, int level) => AllFamilyFacts.Any(f => f.Location.Equals(loc, level));

        public bool BothParentsAlive(FactDate when)
        {
            if (Husband is null || Wife is null || FamilyType.Equals(SOLOINDIVIDUAL))
                return false;
            return Husband.IsAlive(when) && Wife.IsAlive(when) && Husband.GetAge(when).MinAge > 13 && Wife.GetAge(when).MinAge > 13;
        }

        IEnumerable<Fact> AllFamilyFacts
        {
            get
            {
                var results = new List<IList<Fact>>
                {
                    // add the family facts then the facts from each individual
                    Facts
                };
                if (Husband != null)
                    results.Add(Husband.PersonalFacts);
                if (Wife != null)
                    results.Add(Wife.PersonalFacts);
                foreach (var c in Children)
                    results.Add(c.PersonalFacts);
                return results.SelectMany(x => x);
            }
        }

        public IEnumerable<DisplayFact> AllDisplayFacts
        {
            get
            {
                var results = new List<DisplayFact>();
                string surname, forenames;
                if (Husband is null)
                {
                    if (Wife is null)
                    {
                        surname = string.Empty;
                        forenames = string.Empty;
                    }
                    else
                    {
                        surname = Wife.Surname;
                        forenames = Wife.Forenames;
                    }
                }
                else
                {
                    if (Wife is null)
                    {
                        surname = Husband.Surname;
                        forenames = Husband.Forenames;
                    }
                    else
                    {
                        surname = Husband.Surname;
                        forenames = Husband.Forenames + " & " + Wife.Forenames;
                    }
                }

                results.AddRange(Facts.Select(f => new DisplayFact(null, surname, forenames, f)));

                if (Husband != null)
                    results.AddRange(Husband.PersonalFacts.Select(f => new DisplayFact(Husband, f)));

                if (Wife != null)
                    results.AddRange(Wife.PersonalFacts.Select(f => new DisplayFact(Wife, f)));

                foreach (var child in Children)
                {
                    results.AddRange(child.GetFacts(Fact.BIRTH).Select(f => new DisplayFact(child, f)));
                    results.AddRange(child.GetFacts(Fact.BAPTISM).Select(f => new DisplayFact(child, f)));
                    results.AddRange(child.GetFacts(Fact.CHRISTENING).Select(f => new DisplayFact(child, f)));
                }

                return results;
            }
        }

        public override string ToString()
        {
            var marriage = GetPreferredFact(Fact.MARRIAGE);
            return marriage is null ? string.Empty :
                    marriage.Location.IsBlank ? $"{MarriageDate}" :
                        $"{MarriageDate} at {marriage.Location}";
        }

        public IComparer<IDisplayFamily> GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "FamilyID": return CompareComparableProperty<IDisplayFamily>(f => f.FamilyID, ascending);
                case "HusbandID": return CompareComparableProperty<IDisplayFamily>(f => f.HusbandID, ascending);
                case "Husband": return CompareComparableProperty<IDisplayFamily>(f => f.Husband, ascending);
                case "WifeID": return CompareComparableProperty<IDisplayFamily>(f => f.WifeID, ascending);
                case "Wife": return CompareComparableProperty<IDisplayFamily>(f => f.Wife, ascending);
                case "Marriage": return CompareComparableProperty<IDisplayFamily>(f => f.Marriage, ascending);
                case "Location": return CompareComparableProperty<IDisplayFamily>(f => f.Location, ascending);
                case "Children": return CompareComparableProperty<IDisplayFamily>(f => f.Children, ascending);
                case "FamilySize": return CompareComparableProperty<IDisplayFamily>(f => f.FamilySize, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareComparableProperty<T>(Func<IDisplayFamily, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as IDisplayFamily);
                var c2 = accessor(y as IDisplayFamily);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}