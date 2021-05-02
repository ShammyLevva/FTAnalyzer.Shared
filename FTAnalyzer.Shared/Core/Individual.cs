using FTAnalyzer.Exports;
using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer
{
    public class Individual : IComparable<Individual>,
        IDisplayIndividual, IDisplayLooseDeath, IDisplayLooseBirth, IExportIndividual,
        IDisplayColourCensus, IDisplayColourBMD, IDisplayMissingData, IDisplayLooseInfo,
        IJsonIndividual
    {
        // edefine relation type from direct ancestor to related by marriage and 
        // MARRIAGEDB ie: married to a direct or blood relation
        public const int UNKNOWN = 1, DIRECT = 2, DESCENDANT = 4, BLOOD = 8, MARRIEDTODB = 16, MARRIAGE = 32, LINKED = 64, UNSET = 128;
        public const string UNKNOWN_NAME = "UNKNOWN";

        public string IndividualID { get; private set; }
        string _forenames;
        string _fullname;
        string _gender;
        int _relationType;
        List<Fact> _allfacts;
        List<Fact> _allFileFacts;
        readonly DoubleMetaphone surnameMetaphone;
        readonly DoubleMetaphone forenameMetaphone;
        readonly Dictionary<string, Fact> preferredFacts;
        public string Notes { get; private set; }
        public string StandardisedName { get; private set; }
        public bool HasParents { get; set; }
        public bool HasOnlyOneParent { get; set; }
        public bool Infamily { get; set; }
        public bool IsFlaggedAsLiving { get; private set; }
        public BigInteger Ahnentafel { get; set; }
        public string BudgieCode { get; set; }
        public string RelationToRoot { get; set; }
        public string Title { get; private set; }
        public string Suffix { get; private set; }
        public string FamilySearchID { get; private set; }
        public decimal RelationSort { get; set; }
        public CommonAncestor CommonAncestor { get; set; }
        public string UnrecognisedCensusNotes { get; private set; }
        public IList<Fact> Facts { get; private set; }
        public string Alias { get; set; }
        public IList<FactLocation> Locations { get; }

        #region Constructors
        Individual()
        {
            IndividualID = string.Empty;
            _forenames = string.Empty;
            Surname = string.Empty;
            forenameMetaphone = new DoubleMetaphone();
            surnameMetaphone = new DoubleMetaphone();
            MarriedName = string.Empty;
            StandardisedName = string.Empty;
            UnrecognisedCensusNotes = string.Empty;
            FamilySearchID = string.Empty;
            IsFlaggedAsLiving = false;
            Gender = "U";
            Alias = string.Empty;
            Title = string.Empty;
            Suffix = string.Empty;
            Ahnentafel = 0;
            BudgieCode = string.Empty;
            _relationType = UNSET;
            RelationToRoot = string.Empty;
            CommonAncestor = null;
            Infamily = false;
            Notes = string.Empty;
            HasParents = false;
            HasOnlyOneParent = false;
            ReferralFamilyID = string.Empty;
            Facts = new List<Fact>();
            ErrorFacts = new List<Fact>();
            Locations = new List<FactLocation>();
            FamiliesAsChild = new List<ParentalRelationship>();
            FamiliesAsSpouse = new List<Family>();
            preferredFacts = new Dictionary<string, Fact>();
            _allfacts = null;
            _allFileFacts = null;
        }

        public Individual(XmlNode node, IProgress<string> outputText)
            : this()
        {
            IndividualID = node.Attributes["ID"].Value;
            Name = FamilyTree.GetText(node, "NAME", false);
            Gender = FamilyTree.GetText(node, "SEX", false);
            Alias = FamilyTree.GetText(node, "ALIA", false);
            XmlNode nameNode = node?.SelectSingleNode("NAME");
            Title = FamilyTree.GetText(nameNode, "NPFX", false);
            Suffix = FamilyTree.GetText(nameNode, "NSFX", false);
            if (string.IsNullOrEmpty(Alias))
                Alias = FamilyTree.GetText(nameNode, "NICK", false);
            FamilySearchID = FamilyTree.GetText(node, "FSID", false);
            IsFlaggedAsLiving = node.SelectSingleNode("_FLGS/__LIVING") != null;
            forenameMetaphone = new DoubleMetaphone(Forename);
            surnameMetaphone = new DoubleMetaphone(Surname);
            Notes = FamilyTree.GetNotes(node);
            StandardisedName = FamilyTree.Instance.GetStandardisedName(IsMale, Forename);
            Fact nameFact = new Fact(IndividualID, Fact.INDI, FactDate.UNKNOWN_DATE, FactLocation.BLANK_LOCATION,Name, true, true, this);
            AddFact(nameFact);
            // Individual attributes
            AddFacts(node, Fact.NAME, outputText);
            AddFacts(node, Fact.AFN, outputText);
            AddFacts(node, Fact.ALIAS, outputText);
            AddFacts(node, Fact.DEGREE, outputText);
            AddFacts(node, Fact.EDUCATION, outputText);
            AddFacts(node, Fact.EMAIL, outputText);
            AddFacts(node, Fact.FAMILYSEARCH_ID, outputText);
            AddFacts(node, Fact.HEIGHT, outputText);
            AddFacts(node, Fact.MEDICAL_CONDITION, outputText);
            AddFacts(node, Fact.NAMESAKE, outputText);
            AddFacts(node, Fact.NATIONALITY, outputText);
            AddFacts(node, Fact.NAT_ID_NO, outputText);
            AddFacts(node, Fact.NUM_CHILDREN, outputText);
            AddFacts(node, Fact.NUM_MARRIAGE, outputText);
            AddFacts(node, Fact.OCCUPATION, outputText);
            AddFacts(node, Fact.ORIGIN, outputText);
            AddFacts(node, Fact.PHONE, outputText);
            AddFacts(node, Fact.PHYSICAL_DESC, outputText);
            AddFacts(node, Fact.PROPERTY, outputText);
            AddFacts(node, Fact.REFERENCE, outputText);
            AddFacts(node, Fact.SOCIAL_SECURITY, outputText);
            AddFacts(node, Fact.TITLE, outputText);
            AddFacts(node, Fact.WEIGHT, outputText);
            AddFacts(node, Fact.RACE, outputText);

            // Individual events - key facts first
            AddFacts(node, Fact.BIRTH, outputText);
            AddFacts(node, Fact.BIRTH_CALC, outputText);
            AddFacts(node, Fact.DEATH, outputText);
            AddFacts(node, Fact.CENSUS, outputText);

            // Individuals events non key facts
            AddFacts(node, Fact.ADOPTION, outputText);
            AddFacts(node, Fact.ADULT_CHRISTENING, outputText);
            AddFacts(node, Fact.ANNULMENT, outputText);
            AddFacts(node, Fact.BAPTISM, outputText);
            AddFacts(node, Fact.BAPTISM_LDS, outputText);
            AddFacts(node, Fact.BAR_MITZVAH, outputText);
            AddFacts(node, Fact.BAS_MITZVAH, outputText);
            AddFacts(node, Fact.BLESSING, outputText);
            AddFacts(node, Fact.BURIAL, outputText);
            AddFacts(node, Fact.CASTE, outputText);
            AddFacts(node, Fact.CAUSE_OF_DEATH, outputText);
            AddFacts(node, Fact.CHRISTENING, outputText);
            AddFacts(node, Fact.CIRCUMCISION, outputText);
            AddFacts(node, Fact.CONFIRMATION, outputText);
            AddFacts(node, Fact.CONFIRMATION_LDS, outputText);
            AddFacts(node, Fact.CREMATION, outputText);
            AddFacts(node, Fact.DESTINATION, outputText);
            AddFacts(node, Fact.DIVORCE, outputText);
            AddFacts(node, Fact.DIVORCE_FILED, outputText);
            AddFacts(node, Fact.DNA, outputText);
            AddFacts(node, Fact.ELECTION, outputText);
            AddFacts(node, Fact.EMIGRATION, outputText);
            AddFacts(node, Fact.EMPLOYMENT, outputText);
            AddFacts(node, Fact.ENDOWMENT_LDS, outputText);
            AddFacts(node, Fact.ENGAGEMENT, outputText);
            AddFacts(node, Fact.EXCOMMUNICATION, outputText);
            AddFacts(node, Fact.FIRST_COMMUNION, outputText);
            AddFacts(node, Fact.FUNERAL, outputText);
            AddFacts(node, Fact.GRADUATION, outputText);
            AddFacts(node, Fact.IMMIGRATION, outputText);
            AddFacts(node, Fact.INITIATORY_LDS, outputText);
            AddFacts(node, Fact.LEGATEE, outputText);
            AddFacts(node, Fact.MILITARY, outputText);
            AddFacts(node, Fact.MISSION_LDS, outputText);
            AddFacts(node, Fact.NATURALIZATION, outputText);
            AddFacts(node, Fact.OBITUARY, outputText);
            AddFacts(node, Fact.ORDINANCE, outputText);
            AddFacts(node, Fact.ORDINATION, outputText);
            AddFacts(node, Fact.PROBATE, outputText);
            AddFacts(node, Fact.RELIGION, outputText);
            AddFacts(node, Fact.RESIDENCE, outputText);
            AddFacts(node, Fact.RETIREMENT, outputText);
            AddFacts(node, Fact.SEALED_TO_PARENTS, outputText);
            AddFacts(node, Fact.SERVICE_NUMBER, outputText);
            AddFacts(node, Fact.WILL, outputText);

            AddNonStandardFacts(node, outputText);

            // Custom facts
            AddFacts(node, Fact.CUSTOM_EVENT, outputText);
            AddFacts(node, Fact.CUSTOM_FACT, outputText);
            AddFacts(node, Fact.UNKNOWN, outputText);

            if (GeneralSettings.Default.AutoCreateCensusFacts)
            {
                AddCensusSourceFacts();
                AddCensusNoteFacts();
            }
            AddIndividualsSources(node, outputText);
        }

        internal Individual(Individual i)
        {
            if (i != null)
            {
                IndividualID = i.IndividualID;
                _forenames = i._forenames;
                Surname = i.Surname;
                forenameMetaphone = i.forenameMetaphone;
                surnameMetaphone = i.surnameMetaphone;
                MarriedName = i.MarriedName;
                StandardisedName = i.StandardisedName;
                _fullname = i._fullname;
                SortedName = i.SortedName;
                IsFlaggedAsLiving = i.IsFlaggedAsLiving;
                _gender = i._gender;
                Alias = i.Alias;
                Ahnentafel = i.Ahnentafel;
                BudgieCode = i.BudgieCode;
                _relationType = i._relationType;
                RelationToRoot = i.RelationToRoot;
                FamilySearchID = i.FamilySearchID;
                Infamily = i.Infamily;
                Notes = i.Notes;
                HasParents = i.HasParents;
                HasOnlyOneParent = i.HasOnlyOneParent;
                ReferralFamilyID = i.ReferralFamilyID;
                CommonAncestor = i.CommonAncestor;
                Facts = new List<Fact>(i.Facts);
                ErrorFacts = new List<Fact>(i.ErrorFacts);
                Locations = new List<FactLocation>(i.Locations);
                FamiliesAsChild = new List<ParentalRelationship>(i.FamiliesAsChild);
                FamiliesAsSpouse = new List<Family>(i.FamiliesAsSpouse);
                preferredFacts = new Dictionary<string, Fact>(i.preferredFacts);
            }
        }
        #endregion

        #region Properties

        public bool HasRangedBirthDate => BirthDate.DateType == FactDate.FactDateType.BET && BirthDate.StartDate.Year != BirthDate.EndDate.Year;

        public bool HasLostCousinsFactAtDate(CensusDate date)
        {
            foreach (Fact f in AllFacts)
                if (f.FactType == Fact.LOSTCOUSINS || f.FactType == Fact.LC_FTA)
                    if (f.FactDate.Overlaps(date))
                        return true;
            return false;
        }

        public bool HasLostCousinsFact
        {
            get
            {
                foreach (Fact f in AllFacts)
                    if (f.FactType == Fact.LOSTCOUSINS || f.FactType == Fact.LC_FTA)
                        return true;
                return false;
            }
        }

        public int RelationType
        {
            get => _relationType;
            set
            {
                if (_relationType == UNKNOWN || _relationType > value)
                    _relationType = value;
            }
        }

        public bool IsBloodDirect => _relationType == BLOOD || _relationType == DIRECT || _relationType == DESCENDANT || _relationType == MARRIEDTODB;

        public bool HasNotes => Notes.Length > 0;
        public string HasNotesMac => HasNotes ? "Yes" : "No";

        public string Relation
        {
            get
            {
                switch (_relationType)
                {
                    case DIRECT: return Ahnentafel == 1 ? "Root Person" : "Direct Ancestor";
                    case BLOOD: return "Blood Relation";
                    case MARRIAGE: return "By Marriage";
                    case MARRIEDTODB: return "Marr to Direct/Blood";
                    case DESCENDANT: return "Descendant";
                    case LINKED: return "Linked by Marriages";
                    default: return "Unknown";
                }
            }
        }

        public IList<Fact> PersonalFacts => Facts;

        IList<Fact> FamilyFacts
        {
            get
            {
                var familyfacts = new List<Fact>();
                foreach (Family f in FamiliesAsSpouse)
                    familyfacts.AddRange(f.Facts);
                return familyfacts;
            }
        }

        public IList<Fact> ErrorFacts { get; }

        int Factcount { get; set; }
        public IList<Fact> AllFacts
        {
            get
            {
                int currentFactCount = Facts.Count + FamilyFacts.Count;
                if (_allfacts is null || currentFactCount != Factcount)
                {
                    _allfacts = new List<Fact>();
                    _allfacts.AddRange(PersonalFacts);
                    _allfacts.AddRange(FamilyFacts);
                    _allFileFacts = _allfacts.Where(x => !x.Created).ToList();
                    Factcount = _allfacts.Count;
                }
                return _allfacts;
            }
        }

        public IList<Fact> AllFileFacts => _allFileFacts;

        public IList<IDisplayFact> AllGeocodedFacts
        {
            get
            {
                List<IDisplayFact> allGeocodedFacts = new List<IDisplayFact>();
                foreach (Fact f in AllFacts)
                    if (f.Location.IsGeoCoded(false) && f.Location.GeocodeStatus != FactLocation.Geocode.UNKNOWN)
                        allGeocodedFacts.Add(new DisplayFact(this, f));
                allGeocodedFacts.Sort();
                return allGeocodedFacts;
            }
        }

        public IList<IDisplayFact> AllLifeLineFacts
        {
            get
            {
                List<IDisplayFact> allLifeLineFacts = new List<IDisplayFact>();
                foreach (Fact f in AllFacts)
                    if (f.Location.IsGeoCoded(false) && f.Location.GeocodeStatus != FactLocation.Geocode.UNKNOWN && f.FactType != Fact.LC_FTA && f.FactType != Fact.LOSTCOUSINS)
                        allLifeLineFacts.Add(new DisplayFact(this, f));
                allLifeLineFacts.Sort();
                return allLifeLineFacts;
            }
        }

        public int GeoLocationCount => AllGeocodedFacts.Count;


        public string Gender
        {
            get => _gender;
            private set
            {
                _gender = value;
                if (_gender.Length == 0)
                    _gender = "U";
            }
        }

        public bool GenderMatches(Individual that) => Gender == that.Gender || Gender == "U" || that.Gender == "U";

        public string SortedName { get; private set; }

        public string Name
        {
            get => _fullname;
            private set
            {
                string name = value;
                int startPos = name.IndexOf("/", StringComparison.Ordinal), endPos = name.LastIndexOf("/", StringComparison.Ordinal);
                if (startPos >= 0 && endPos > startPos)
                {
                    Surname = name.Substring(startPos + 1, endPos - startPos - 1);
                    _forenames = startPos == 0 ? UNKNOWN_NAME : name.Substring(0, startPos).Trim();
                }
                else
                {
                    Surname = UNKNOWN_NAME;
                    _forenames = name;
                }
                if (string.IsNullOrEmpty(Surname) || Surname.ToLower() == "mnu" || Surname.ToLower() == "lnu" || Surname == "[--?--]" || Surname.ToLower() == "unk" ||
                  ((Surname[0] == '.' || Surname[0] == '?' || Surname[0] == '_') && Surname.Distinct().Count() == 1)) // if all chars are same and is . ? or _
                    Surname = UNKNOWN_NAME;
                if (GeneralSettings.Default.TreatFemaleSurnamesAsUnknown && !IsMale && Surname.StartsWith("(", StringComparison.Ordinal) && Surname.EndsWith(")", StringComparison.Ordinal))
                    Surname = UNKNOWN_NAME;
                if(string.IsNullOrEmpty(_forenames) || _forenames.ToLower() == "unk" || _forenames == "[--?--]" ||
                  ((_forenames[0] == '.' || _forenames[0] == '?' || _forenames[0] == '_') && _forenames.Distinct().Count() == 1))
                  _forenames = UNKNOWN_NAME;
                MarriedName = Surname;
                _fullname = SetFullName();
                SortedName = $"{_forenames} {Surname}".Trim();
            }
        }

        public string SetFullName()
        {
            return GeneralSettings.Default.ShowAliasInName && Alias.Length > 0
                ? $"{_forenames}  '{Alias}' {Surname}".Trim()
                : $"{_forenames} {Surname}".Trim();
        }

        public string Forename
        {
            get
            {
                if (_forenames is null)
                    return string.Empty;
                int pos = _forenames.IndexOf(" ", StringComparison.Ordinal);
                return pos > 0 ? _forenames.Substring(0, pos) : _forenames;
            }
        }

        public string OtherNames
        {
            get
            {
                if (_forenames is null)
                    return string.Empty;
                int pos = _forenames.IndexOf(" ", StringComparison.Ordinal);
                return pos > 0 ? _forenames.Substring(pos).Trim() : string.Empty;
            }
        }

        public string ForenameMetaphone => forenameMetaphone.PrimaryKey;

        public string Forenames => GeneralSettings.Default.ShowAliasInName && Alias.Length > 0 ? $"{_forenames} '{Alias}' " : _forenames;

        public string Surname { get; private set; }

        public string SurnameMetaphone => surnameMetaphone.PrimaryKey;

        public string MarriedName { get; set; }

        public Fact BirthFact
        {
            get
            {
                Fact f = GetPreferredFact(Fact.BIRTH);
                if (f != null)
                    return f;
                f = GetPreferredFact(Fact.BIRTH_CALC);
                if (GeneralSettings.Default.UseBaptismDates)
                {
                    if (f != null)
                        return f;
                    f = GetPreferredFact(Fact.BAPTISM);
                    if (f != null)
                        return f;
                    f = GetPreferredFact(Fact.CHRISTENING);
                }
                return f;
            }
        }

        public FactDate BirthDate => BirthFact is null ? FactDate.UNKNOWN_DATE : BirthFact.FactDate;

        public DateTime BirthStart => BirthDate.StartDate != FactDate.MINDATE ? BirthDate.StartDate : BirthDate.EndDate;
        public DateTime BirthEnd => BirthDate.StartDate != FactDate.MAXDATE ? BirthDate.EndDate : BirthDate.StartDate;

        public FactLocation BirthLocation => (BirthFact is null) ? FactLocation.BLANK_LOCATION : BirthFact.Location;

        public Fact DeathFact
        {
            get
            {
                Fact f = GetPreferredFact(Fact.DEATH);
                if (GeneralSettings.Default.UseBurialDates)
                {
                    if (f != null)
                        return f;
                    f = GetPreferredFact(Fact.BURIAL);
                    if (f != null)
                        return f;
                    f = GetPreferredFact(Fact.CREMATION);
                }
                return f;
            }
        }

        public FactDate DeathDate => DeathFact is null ? FactDate.UNKNOWN_DATE : DeathFact.FactDate;

        public DateTime DeathStart => DeathDate.StartDate != FactDate.MINDATE ? DeathDate.StartDate : DeathDate.EndDate;
        public DateTime DeathEnd => DeathDate.EndDate != FactDate.MAXDATE ? DeathDate.EndDate : DeathDate.StartDate;

        public FactLocation DeathLocation => DeathFact is null ? FactLocation.BLANK_LOCATION : DeathFact.Location;

        public FactDate BaptismDate
        {
            get
            {
                Fact f = GetPreferredFact(Fact.BAPTISM);
                if (f is null)
                    f = GetPreferredFact(Fact.CHRISTENING);
                return f?.FactDate;
            }
        }

        public FactDate BurialDate
        {
            get
            {
                Fact f = GetPreferredFact(Fact.BURIAL);
                if (f is null)
                    f = GetPreferredFact(Fact.CREMATION);
                return f?.FactDate;
            }
        }

        public string Occupation
        {
            get
            {
                Fact occupation = GetPreferredFact(Fact.OCCUPATION);
                return occupation is null ? string.Empty : occupation.Comment;
            }
        }

        int MaxAgeAtDeath => DeathDate.EndDate > FactDate.NOW ? GetAge(FactDate.NOW).MaxAge : GetAge(DeathDate).MaxAge;

        public Age LifeSpan => GetAge(FactDate.TODAY);

        public FactDate LooseBirthDate
        {
            get
            {
                Fact loose = GetPreferredFact(Fact.LOOSEBIRTH);
                return loose is null ? FactDate.UNKNOWN_DATE : loose.FactDate;
            }
        }

        public string LooseBirth
        {
            get
            {
                FactDate fd = LooseBirthDate;
                return (fd.StartDate > fd.EndDate) ? "Alive facts after death, check data errors tab and children's births" : fd.ToString();
            }
        }

        public FactDate LooseDeathDate
        {
            get
            {
                Fact loose = GetPreferredFact(Fact.LOOSEDEATH);
                return loose is null ? FactDate.UNKNOWN_DATE : loose.FactDate;
            }
        }

        public string LooseDeath
        {
            get
            {
                FactDate fd = LooseDeathDate;
                return (fd.StartDate > fd.EndDate) ? "Alive facts after death, check data errors tab and children's births" : fd.ToString();
            }
        }

        public string IndividualRef => $"{IndividualID}: {Name}";

        public string ServiceNumber
        {
            get
            {
                Fact service = GetPreferredFact(Fact.SERVICE_NUMBER);
                return service is null ? string.Empty : service.Comment;
            }
        }

        public bool BirthdayEffect
        {
            get
            {
                if (BirthDate.IsExact && DeathDate.IsExact)
                {
                    DateTime amendedDeath;
                    try
                    {
                        if (DeathDate.StartDate.Month == 2 && DeathDate.StartDate.Day == 29)
                            amendedDeath = new DateTime(BirthDate.StartDate.Year, 2, 28); // fix issue with 29th February death dates
                        else
                            amendedDeath = new DateTime(BirthDate.StartDate.Year, DeathDate.StartDate.Month, DeathDate.StartDate.Day); // set death date to be same year as birth
                        var diff = Math.Abs((amendedDeath - BirthDate.StartDate).Days);
                        Console.WriteLine($"Processed Individual: {IndividualID}: {Name}, Diff:{diff}, Birth: {BirthDate.StartDate.ToShortDateString()} Death: {DeathDate.StartDate.ToShortDateString()}");
                        if(diff>180)
                        {
                            if (BirthDate.StartDate.Month < 7)
                                amendedDeath = amendedDeath.AddYears(-1);
                            else
                                amendedDeath = amendedDeath.AddYears(1);
                            diff = Math.Abs((amendedDeath - BirthDate.StartDate).Days);
                        }
                        return diff < 16;
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        Console.WriteLine($"PROBLEM Individual: {IndividualID}: {Name}");
                        return false;
                    }
                }
                return false;
            }
        }

        public string BirthMonth => BirthDate.IsExact ? BirthDate.StartDate.ToString("MM : MMMM", CultureInfo.InvariantCulture) : "00 : Unknown";

        public IList<Family> FamiliesAsSpouse { get; }

        public IList<ParentalRelationship> FamiliesAsChild { get; }

        public string FamilyIDsAsParent {
            get
            {
                string result = string.Empty;
                foreach(Family f in FamiliesAsSpouse)
                    result += f.FamilyID + ",";
                return result.Length == 0 ? result : result.Substring(0, result.Length - 1);
            }
        }

        public string FamilyIDsAsChild
        {
            get
            {
                string result = string.Empty;
                foreach(ParentalRelationship pr in FamiliesAsChild)
                    result += pr.Family.FamilyID + ",";
                return result.Length == 0 ? result : result.Substring(0, result.Length - 1);
            }
        }

        public bool IsNaturalChildOf(Individual parent)
        {
            if (parent is null) return false;
            foreach (ParentalRelationship pr in FamiliesAsChild)
            {
                if (pr.Family != null)
                    return (pr.IsNaturalFather && parent.IsMale && parent.Equals(pr.Father)) ||
                           (pr.IsNaturalMother && !parent.IsMale && parent.Equals(pr.Mother));
            }
            return false;
        }

        public Individual NaturalFather
        {
            get
            {
                foreach (ParentalRelationship pr in FamiliesAsChild)
                {
                    if (pr.Family != null && pr.Father != null && pr.IsNaturalFather)
                        return pr.Father;
                }
                return null;
            }
        }

        public int FactCount(string factType) => Facts.Count(f => f.FactType == factType && f.FactErrorLevel == Fact.FactError.GOOD);

        public int ResidenceCensusFactCount => Facts.Count(f => f.FactType == Fact.RESIDENCE && f.IsCensusFact);

        public int ErrorFactCount(string factType, Fact.FactError errorLevel) => ErrorFacts.Count(f => f.FactType == factType && f.FactErrorLevel == errorLevel);

        public string MarriageDates
        {
            get
            {
                string output = string.Empty;
                foreach (Family f in FamiliesAsSpouse)
                    if (!string.IsNullOrEmpty(f.MarriageDate?.ToString()))
                        output += $"{f.MarriageDate}; ";
                return output.Length > 0 ? output.Substring(0, output.Length - 2) : output; // remove trailing ;
            }
        }

        public string MarriageLocations
        {
            get
            {
                string output = string.Empty;
                foreach (Family f in FamiliesAsSpouse)
                    if (!string.IsNullOrEmpty(f.MarriageLocation))
                        output += $"{f.MarriageLocation}; ";
                return output.Length > 0 ? output.Substring(0, output.Length - 2) : output; // remove trailing ;
            }
        }

        public int MarriageCount => FamiliesAsSpouse.Count;

        public int ChildrenCount => FamiliesAsSpouse.Sum(x => x.Children.Count);

        #endregion

        #region Boolean Tests

        public bool IsMale => _gender.Equals("M");

        public bool IsInFamily => Infamily;

        public bool IsMarried(FactDate fd)
        {
            if (IsSingleAtDeath)
                return false;
            return FamiliesAsSpouse.Any(f =>
            {
                FactDate marriage = f.GetPreferredFactDate(Fact.MARRIAGE);
                return (marriage != null && marriage.IsBefore(fd));
            });
        }

        public bool HasMilitaryFacts => Facts.Any(f => f.FactType == Fact.MILITARY || f.FactType == Fact.SERVICE_NUMBER);

        public bool HasCensusLocation(CensusDate when)
        {
            if (when is null) return false;
            foreach (Fact f in Facts)
            {
                if (f.IsValidCensus(when) && f.Location.ToString().Length > 0)
                    return true;
            }
            return false;
        }

        public Fact CensusFact(FactDate factDate)
        {
            if (factDate is null) return null;
            foreach (Fact f in Facts)
            {
                if (f.IsValidCensus(factDate))
                    return f;
            }
            return null;
        }

        public CensusReference GetCensusReference(FactDate factDate) => CensusFact(factDate)?.CensusReference;

        public bool CensusFactExists(FactDate factDate, bool includeCreated)
        {
            if (factDate is null) return false;
            foreach (Fact f in Facts)
            {
                if (f.IsValidCensus(factDate))
                    return !f.Created || includeCreated;
            }
            return false;
        }

        public bool IsCensusDone(CensusDate when) => IsCensusDone(when, true, true);
        public bool IsCensusDone(CensusDate when, bool includeUnknownCountries) => IsCensusDone(when, includeUnknownCountries, true);
        public bool IsCensusDone(CensusDate when, bool includeUnknownCountries, bool checkCountry)
        {
            if (when is null) return false;
            foreach (Fact f in Facts)
            {
                if (f.IsValidCensus(when))
                {
                    if (!checkCountry)
                        return true;
                    if (f.Location.CensusCountryMatches(when.Country, includeUnknownCountries))
                        return true;
                    if (Countries.IsUnitedKingdom(when.Country) && f.IsOverseasUKCensus(f.Country))
                        return true;
                }
            }
            return false;
        }

        public bool IsTaggedMissingCensus(CensusDate when) => when is object && Facts.Any(x => x.FactType == Fact.MISSING && x.FactDate.Overlaps(when));

        public bool IsLostCousinsEntered(CensusDate when) => !(when is null) && IsLostCousinsEntered(when, true);
        public bool IsLostCousinsEntered(CensusDate when, bool includeUnknownCountries)
        {
            if (when is null) return false;
            foreach (Fact f in Facts)
            {
                if (f.IsValidLostCousins(when))
                {
                    if (f.Location.CensusCountryMatches(when.Country, includeUnknownCountries) || BestLocation(when).CensusCountryMatches(when.Country, includeUnknownCountries))
                        return true;
                    Fact censusFact = GetCensusFact(f);
                    if (censusFact != null)
                    {
                        if (when.Country.Equals(Countries.SCOTLAND) && Countries.IsEnglandWales(censusFact.Country))
                            return false;
                        if (censusFact.Country.Equals(Countries.SCOTLAND) && Countries.IsEnglandWales(when.Country))
                            return false;
                        if (Countries.IsUnitedKingdom(when.Country) && (Countries.IsUnitedKingdom(censusFact.Country) || censusFact.IsOverseasUKCensus(censusFact.Country)))
                            return true;
                    }
                }
            }
            return false;
        }

        public bool HasLostCousinsFactWithNoCensusFact
        {
            get
            {
                foreach (CensusDate censusDate in CensusDate.LOSTCOUSINS_CENSUS)
                {
                    if (IsLostCousinsEntered(censusDate, false) && !IsCensusDone(censusDate))
                        return true;
                }
                return false;
            }
        }

        public bool MissingLostCousins(CensusDate censusDate, bool includeUnknownCountries)
        {
            bool isCensusDone = IsCensusDone(censusDate, includeUnknownCountries);
            bool isLostCousinsEntered = IsLostCousinsEntered(censusDate, includeUnknownCountries);
            return isCensusDone && !isLostCousinsEntered;
        }

        public bool IsAlive(FactDate when) => IsBorn(when) && !IsDeceased(when);

        public bool IsBorn(FactDate when) => !BirthDate.IsKnown || BirthDate.StartsOnOrBefore(when); // assume born if birthdate is unknown

        public bool IsDeceased(FactDate when) => DeathDate.IsKnown && DeathDate.IsBefore(when);

        public bool IsSingleAtDeath => GetPreferredFact(Fact.UNMARRIED) != null || MaxAgeAtDeath < 16 || LifeSpan.MaxAge < 16;

        public bool IsBirthKnown => BirthDate.IsKnown && BirthDate.IsExact;

        public bool IsDeathKnown => DeathDate.IsKnown && DeathDate.IsExact;

        public bool IsPossiblyAlive(FactDate when)
        {
            if (when is null || when.IsUnknown) return true;
            if (BirthDate.StartDate <= when.EndDate && DeathDate.EndDate >= when.StartDate) return true;
            if(DeathDate.IsUnknown)
            {
                // if unknown death add 110 years to Enddate
                var death = BirthDate.AddEndDateYears(110);
                if (BirthDate.StartDate <= when.EndDate && death.EndDate >= when.StartDate) return true;
            }
            return false;
        }

        #endregion

        #region Age Functions

        public Age GetAge(FactDate when) => new Age(this, when);

        public Age GetAge(FactDate when, string factType) => (factType == Fact.BIRTH || factType == Fact.PARENT) ? Age.BIRTH : new Age(this, when);

        public Age GetAge(DateTime when)
        {
            string now = FactDate.Format(FactDate.FULL, when);
            return GetAge(new FactDate(now));
        }

        public int GetMaxAge(FactDate when) => GetAge(when).MaxAge;

        public int GetMinAge(FactDate when) => GetAge(when).MinAge;

        public int GetMaxAge(DateTime when)
        {
            string now = FactDate.Format(FactDate.FULL, when);
            return GetMaxAge(new FactDate(now));
        }

        public int GetMinAge(DateTime when)
        {
            string now = FactDate.Format(FactDate.FULL, when);
            return GetMinAge(new FactDate(now));
        }
        #endregion

        #region Fact Functions

        void AddIndividualsSources(XmlNode node, IProgress<string> outputText)
        {
            Fact nameFact = GetFacts(Fact.INDI).First();
            // now iterate through source elements of the fact finding all sources
            XmlNodeList list = node.SelectNodes("SOUR");
            foreach (XmlNode n in list)
            {
                if (n.Attributes["REF"] != null)
                {   // only process sources with a reference
                    string srcref = n.Attributes["REF"].Value;
                    FactSource source = FamilyTree.Instance.GetSource(srcref);
                    if (source != null)
                    {
                        nameFact.Sources.Add(source);
                        source.AddFact(nameFact);
                    }
                    else
                        outputText.Report($"Source {srcref} not found.\n");
                }
            }
        }

        void AddFacts(XmlNode node, string factType, IProgress<string> outputText) => AddFacts(node, factType, outputText, null);
        
        void AddFacts(XmlNode node, string factType, IProgress<string> outputText, string nonStandardFactType)
        {
            XmlNodeList list = nonStandardFactType != null ? node.SelectNodes(nonStandardFactType) : node.SelectNodes(factType);
            bool preferredFact = true;
            foreach (XmlNode n in list)
            {
                try
                {
                    if (preferredFact || GeneralSettings.Default.IncludeAlternateFacts || Fact.IsAlwaysLoadFact(factType))
                    {  // add only preferred facts or all census facts
                        if (!preferredFact || factType != Fact.NAME)
                        { // skip first name face as already added
                            Fact f = new Fact(n, this, preferredFact, null, outputText);
                            if (f.FactType == Fact.NAME && string.IsNullOrEmpty(Alias))
                                Alias = f.Comment;
                            if (f.FactDate.SpecialDate)
                                ProcessSpecialDate(n, f, preferredFact, outputText);
                            if (nonStandardFactType != null)
                                f.ChangeNonStandardFactType(factType);
                            f.Location.FTAnalyzerCreated = false;
                            if (!f.Location.IsValidLatLong)
                                outputText.Report($"Found problem with Lat/Long for Location '{f.Location}' in facts for {IndividualID}: {Name}");
                            AddFact(f);
                            if (f.GedcomAge != null && f.GedcomAge.CalculatedBirthDate != FactDate.UNKNOWN_DATE)
                            {
                                string reason = $"Calculated from {f} with Age: {f.GedcomAge.GEDCOM_Age}";
                                Fact calculatedBirth = new Fact(IndividualID, Fact.BIRTH_CALC, f.GedcomAge.CalculatedBirthDate, FactLocation.UNKNOWN_LOCATION, reason, false, true);
                                AddFact(calculatedBirth);
                            }
                        }
                    }
                }
                catch (InvalidXMLFactException ex)
                {
                    outputText.Report($"Error with Individual : {IndividualRef}\n       Invalid fact : {ex.Message}");
                }
                preferredFact = false;
            }
        }

        void ProcessSpecialDate(XmlNode n, Fact addedFact, bool preferredFact, IProgress<string> outputText)
        {
            if (BirthDate.IsKnown)
            {
                int years;
                switch (addedFact.FactDate.OriginalString.ToUpper())
                {
                    case "STILLBORN":
                        years = 0;
                        break;
                    case "INFANT":
                        years = 5;
                        break;
                    case "CHILD":
                        years = 14;
                        break;
                    case "YOUNG":
                        years = 21;
                        break;
                    case "UNMARRIED":
                    case "NEVER MARRIED":
                    case "NOT MARRIED":
                        years = -2;
                        break;
                    default:
                        years = -1;
                        break;
                }
                if (years >= 0 && addedFact.FactType == Fact.DEATH)  //only add a death fact if text is one of the death types
                {
                    FactDate deathdate = BirthDate.AddEndDateYears(years);
                    Fact f = new Fact(n, this, preferredFact, deathdate, outputText);
                    AddFact(f);
                }
                else
                {
                    Fact f = new Fact(n, this, preferredFact, FactDate.UNKNOWN_DATE, outputText); // write out death fact with unknown date
                    AddFact(f);
                    f = new Fact(string.Empty, Fact.UNMARRIED, FactDate.UNKNOWN_DATE, FactLocation.UNKNOWN_LOCATION, string.Empty, true, true);
                    AddFact(f);
                }
            }
        }
        void AddNonStandardFacts(XmlNode node, IProgress<string> outputText)
        {
            foreach(KeyValuePair<string, string> factType in Fact.NON_STANDARD_FACTS)
            {
                AddFacts(node, factType.Value, outputText, factType.Key);
            }
        }

        public void AddFact(Fact fact)
        {
            if (fact is null)
                return;
            if (FamilyTree.FactBeforeBirth(this, fact))
                fact.SetError((int)FamilyTree.Dataerror.FACTS_BEFORE_BIRTH, Fact.FactError.ERROR,
                    $"{fact.FactTypeDescription} fact recorded: {fact.FactDate} before individual was born");
            if (FamilyTree.FactAfterDeath(this, fact))
                fact.SetError((int)FamilyTree.Dataerror.FACTS_AFTER_DEATH, Fact.FactError.ERROR,
                    $"{fact.FactTypeDescription} fact recorded: {fact.FactDate} after individual died");

            switch (fact.FactErrorLevel)
            {
                case Fact.FactError.GOOD:
                    AddGoodFact(fact);
                    break;
                case Fact.FactError.WARNINGALLOW:
                    AddGoodFact(fact);
                    ErrorFacts.Add(fact);
                    break;
                case Fact.FactError.WARNINGIGNORE:
                case Fact.FactError.ERROR:
                    ErrorFacts.Add(fact);
                    break;
            }
        }

        void AddGoodFact(Fact fact)
        {
            Facts.Add(fact);
            if (fact.Preferred && !preferredFacts.ContainsKey(fact.FactType))
                preferredFacts.Add(fact.FactType, fact);
            AddLocation(fact);
        }

        /// <summary>
        /// Checks the individual's node data to see if any census references exist in the source records
        /// </summary>
        void AddCensusSourceFacts()
        {
            List<Fact> toAdd = new List<Fact>(); // we can't vary the facts collection whilst looping
            foreach (Fact f in Facts)
            {
                if (!f.IsCensusFact && !CensusFactExists(f.FactDate, true))
                {
                    foreach (FactSource s in f.Sources)
                    {
                        CensusReference cr = new CensusReference(IndividualID, $"{s.SourceTitle} {s.SourceText}", true);
                        if (OKtoAddReference(cr, true))
                        {
                            cr.Fact.Sources.Add(s);
                            toAdd.Add(cr.Fact);
                            if (cr.IsLCCensusFact)
                                CreateLCFact(toAdd, cr);
                        }
                        else
                            UpdateCensusFactReference(cr);
                    }
                }
            }
            foreach (Fact f in toAdd)
                AddFact(f);
        }

        void CreateLCFact(List<Fact> toAdd, CensusReference cr)
        {
            if (!IsLostCousinsEntered((CensusDate)cr.Fact.FactDate))
            {
                Fact lcFact = new Fact("LostCousins", Fact.LC_FTA, cr.Fact.FactDate, cr.Fact.Location, "Lost Cousins fact created by FTAnalyzer by recognising census ref " + cr.Reference, false, true);
                if (toAdd is null)
                    AddFact(lcFact);
                else
                    toAdd.Add(lcFact);
            }
        }

        public string LCSurnameAtDate(CensusDate date) => ValidLostCousinsString(SurnameAtDate(date), false);
        public string LCSurname => ValidLostCousinsString(Surname, false);
        public string LCForename => ValidLostCousinsString(Forename, false);
        public string LCOtherNames => ValidLostCousinsString(OtherNames, true);

        string ValidLostCousinsString(string input, bool allowspace)
        {
            StringBuilder output = new StringBuilder();
            input = RemoveQuoted(input);
            foreach (char c in input)
            {
                if (c == '-' || c == '\'' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    output.Append(c);
                if (allowspace && c == ' ')
                    output.Append(c);
            }
            var result = output.ToString().Replace("--","-").Replace("--", "-").Replace("--", "-");
            return result == "-" ? UNKNOWN_NAME : result;
        }

        string RemoveQuoted(string input)
        {
            string output = input.Replace("UNKNOWN", "");
            int startptr = input.IndexOf('\'');
            if (startptr == -1) startptr = input.IndexOf('\"');
            if (startptr != -1)
            {
                int endptr = input.IndexOf('\'', startptr);
                if (endptr == -1) endptr = input.IndexOf('\"', startptr);
                output = (startptr < input.Length ? input.Substring(0, startptr) : string.Empty) + (endptr < input.Length ? input.Substring(endptr) : string.Empty);
            }
            output = output.Replace("--","").Replace('\'', ' ').Replace('\"', ' ').Replace("  ", " ").Replace("  ", " ");
            return output.TrimEnd('-').Trim();
        }


        /// <summary>
        /// Checks the notes against an individual to see if any census data exists
        /// </summary>
        void AddCensusNoteFacts()
        {
            if (HasNotes)
            {
                bool checkNotes = true;
                string notes = CensusReference.ClearCommonPhrases(Notes);
                notes = notes.ClearWhiteSpace();
                while (checkNotes)
                {
                    checkNotes = false;
                    CensusReference cr = new CensusReference(IndividualID, notes, false);
                    if (OKtoAddReference(cr, false))
                    {   // add census fact even if other created census facts exist for that year
                        AddFact(cr.Fact);
                        if (cr.IsLCCensusFact)
                            CreateLCFact(null, cr);
                    }
                    else
                        UpdateCensusFactReference(cr);
                    if (cr.MatchString.Length > 0)
                    {
                        int pos = notes.IndexOf(cr.MatchString, StringComparison.OrdinalIgnoreCase);
                        if (pos != -1)
                        {
                            notes = notes.Remove(pos, cr.MatchString.Length);
                            checkNotes = notes.Length > 10 && cr.MatchString.Length > 0;
                        }
                    }
                }
                if (notes.Length > 10) // no point recording really short notes 
                    UnrecognisedCensusNotes = IndividualID + ": " + Name + ". Notes : " + notes;
            }
        }

        void UpdateCensusFactReference(CensusReference cr)
        {
            Fact censusFact = GetCensusFact(cr.Fact, false);
            if (censusFact != null && !censusFact.CensusReference.IsKnownStatus && cr.IsKnownStatus)
                censusFact.SetCensusReferenceDetails(cr, CensusLocation.UNKNOWN, string.Empty);
        }

        bool OKtoAddReference(CensusReference cr, bool includeCreated) => cr.IsKnownStatus && !CensusFactExists(cr.Fact.FactDate, includeCreated) && IsPossiblyAlive(cr.Fact.FactDate);

        void AddLocation(Fact fact)
        {
            FactLocation loc = fact.Location;
            if (loc != null && !Locations.ContainsLocation(loc))
            {
                Locations.Add(loc);
                loc.AddIndividual(this);
            }
        }

        public Fact GetPreferredFact(string factType) => preferredFacts.ContainsKey(factType) ? preferredFacts[factType] : Facts.FirstOrDefault(f => f.FactType == factType);

        public FactDate GetPreferredFactDate(string factType)
        {
            Fact f = GetPreferredFact(factType);
            return (f is null || f.FactDate is null) ? FactDate.UNKNOWN_DATE : f.FactDate;
        }
        
        // Returns all facts of the given type.
        public IEnumerable<Fact> GetFacts(string factType) => Facts.Where(f => f.FactType == factType);

        public string SurnameAtDate(FactDate date)
        {
            string name = Surname;
            if (!IsMale)
            {
                foreach (Family marriage in FamiliesAsSpouse.OrderBy(f => f.MarriageDate))
                {
                    if ((marriage.MarriageDate.Equals(date) || marriage.MarriageDate.IsBefore(date)) && marriage.Husband != null && marriage.Husband.Surname != "UNKNOWN")
                        name = marriage.Husband.Surname;
                }
            }
            return name;
        }

        public void QuestionGender(Family family, bool pHusband)
        {
            if (family is null) return;
            string description;
            if (Gender.Equals("U"))
            {
                string spouse = pHusband ? "husband" : "wife";
                description = $"Unknown gender but appears as a {spouse} in family {family.FamilyRef} check gender setting";
            }
            else
            {
                if (IsMale)
                    description = $"Male but appears as a wife in family {family.FamilyRef} check family for swapped husband and wife";
                else
                    description = $"Female but appears as husband in family {family.FamilyRef} check family for swapped husband and wife";
            }
            var gender = new Fact(family.FamilyID, Fact.GENDER, FactDate.UNKNOWN_DATE, null, description, true, true);
            gender.SetError((int)FamilyTree.Dataerror.MALE_WIFE_FEMALE_HUSBAND, Fact.FactError.ERROR, description);
            AddFact(gender);
        }
        #endregion

        #region Location functions

        public FactLocation BestLocation(FactDate when) => FactLocation.BestLocation(AllFacts, when);  // this returns a Location a person was at for a given period

        public Fact BestLocationFact(FactDate when, int limit) => FactLocation.BestLocationFact(AllFacts, when, limit); // this returns a Fact a person was at for a given period

        public bool IsAtLocation(FactLocation loc, int level)
        {
            foreach (Fact f in AllFacts)
            {
                if (f.Location.Equals(loc, level))
                    return true;
            }
            return false;
        }
        #endregion

        readonly FactComparer factComparer = new FactComparer();

        public int DuplicateLCFacts
        {
            get
            {
                IEnumerable<Fact> lcFacts = AllFacts.Where(f => f.FactType == Fact.LOSTCOUSINS || f.FactType == Fact.LC_FTA);
                int distinctFacts = lcFacts.Distinct(factComparer).Count();
                return LostCousinsFacts - distinctFacts;
            }
        }

        public int DuplicateLCCensusFacts
        {
            get
            {
                IEnumerable<Fact> censusFacts = AllFacts.Where(f => f.IsLCCensusFact);
                int distinctFacts = censusFacts.Distinct(factComparer).Count();
                return censusFacts.Count() - distinctFacts;
            }
        }

        public int LostCousinsFacts => Facts.Count(f => f.FactType == Fact.LOSTCOUSINS || f.FactType == Fact.LC_FTA);

        public string ReferralFamilyID { get; set; }

        public Fact GetCensusFact(Fact lcFact, bool includeCreated = true)
        {
            return includeCreated
                ? Facts.FirstOrDefault(x => x.IsCensusFact && x.FactDate.Overlaps(lcFact.FactDate))
                : Facts.FirstOrDefault(x => x.IsCensusFact && !x.Created && x.FactDate.Overlaps(lcFact.FactDate));
        }

        public void FixIndividualID(int length)
        {
            try
            {
                IndividualID = IndividualID.Substring(0, 1) + IndividualID.Substring(1).PadLeft(length, '0');
            }
            catch (ArgumentOutOfRangeException)
            {  // don't error if Individual isn't of type Ixxxx
            }
        }

        #region Colour Census 
        CensusColours ColourCensusReport(CensusDate census)
        {
            if (BirthDate.IsAfter(census) || DeathDate.IsBefore(census) || GetAge(census).MinAge >= FactDate.MAXYEARS)
                return CensusColours.NOT_ALIVE; // not alive - grey
            if (!IsCensusDone(census))
            {
                if (IsTaggedMissingCensus(census))
                    return CensusColours.KNOWN_MISSING;
                if (IsCensusDone(census, true, false) || (Countries.IsUnitedKingdom(census.Country) && IsCensusDone(census.EquivalentUSCensus, true, false)))
                    return CensusColours.OVERSEAS_CENSUS; // checks if on census outside UK in census year or on prior year (to check US census)
                FactLocation location = BestLocation(census);
                if (CensusDate.IsLostCousinsCensusYear(census, true) && IsLostCousinsEntered(census) && !OutOfCountryCheck(census, location))
                    return CensusColours.LC_PRESENT_NO_CENSUS; // LC entered but no census entered - orange
                if (location.IsKnownCountry)
                {
                    if (OutOfCountryCheck(census, location))
                        return CensusColours.OUT_OF_COUNTRY; // Likely out of country on census date
                    return CensusColours.NO_CENSUS; // no census - red
                }
                return CensusColours.NO_CENSUS; // no census - red
            }
            if (!CensusDate.IsLostCousinsCensusYear(census, true))
                return CensusColours.CENSUS_PRESENT_NOT_LC_YEAR; // census entered but not LCyear - green
            if (IsLostCousinsEntered(census))
                return CensusColours.CENSUS_PRESENT_LC_PRESENT; // census + Lost cousins entered - green
                // we have a census in a LC year but no LC event check if country is a LC country.
            int year = census.StartDate.Year;
            if (year == 1841 && IsCensusDone(CensusDate.EWCENSUS1841, false))
                return CensusColours.CENSUS_PRESENT_LC_MISSING; // census entered LC not entered - yellow
            if (year == 1880 && IsCensusDone(CensusDate.USCENSUS1880, false))
                return CensusColours.CENSUS_PRESENT_LC_MISSING; // census entered LC not entered - yellow
            if (year == 1881 &&
                (IsCensusDone(CensusDate.EWCENSUS1881, false) || IsCensusDone(CensusDate.CANADACENSUS1881, false) ||
                 IsCensusDone(CensusDate.SCOTCENSUS1881, false)))
                return CensusColours.CENSUS_PRESENT_LC_MISSING; // census entered LC not entered - yellow
            if (year == 1911 && (IsCensusDone(CensusDate.EWCENSUS1911, false) || IsCensusDone(CensusDate.IRELANDCENSUS1911, false)))
                return CensusColours.CENSUS_PRESENT_LC_MISSING; // census entered LC not entered - yellow
            if (year == 1940 && IsCensusDone(CensusDate.USCENSUS1940, false))
                return CensusColours.CENSUS_PRESENT_LC_MISSING; // census entered LC not entered - yellow
            return CensusColours.CENSUS_PRESENT_NOT_LC_YEAR;  // census entered and LCyear but not LC country - green
        }

        public bool AliveOnAnyCensus(string country)
        {
            if (country is null) return false;
            int ukCensus = (int)C1841 + (int)C1851 + (int)C1861 + (int)C1871 + (int)C1881 + (int)C1891 + (int)C1901 + (int)C1911 + (int)C1921 + (int)C1939;
            if (country.Equals(Countries.UNITED_STATES))
                return ((int)US1790 + (int)US1800 + (int)US1810 + (int)US1810 + (int)US1820 + (int)US1830 + (int)US1840 + (int)US1850 + (int)US1860 + (int)US1870 + (int)US1880 + (int)US1890 + (int)US1900 + (int)US1910 + (int)US1920 + (int)US1930 + (int)US1940 + (int)US1950) > 0;
            if (country.Equals(Countries.CANADA))
                return ((int)Can1851 + (int)Can1861 + (int)Can1871 + (int)Can1881 + (int)Can1891 + (int)Can1901 + (int)Can1906 + (int)Can1911 + (int)Can1916 + (int)Can1921) > 0;
            if (country.Equals(Countries.IRELAND))
                return ((int)Ire1901 + (int)Ire1911) > 0;
            if (country.Equals(Countries.SCOTLAND))
                return (ukCensus + (int)V1855 + (int)V1865 + (int)V1875 + (int)V1885 + (int)V1895 + (int)V1905 + (int)V1915 + (int)V1920 + (int)V1925 + (int)V1930 + (int)V1935 + (int)V1940) > 0;
            return ukCensus > 0;
        }

        public bool OutOfCountryOnAllCensus(string country)
        {
            if (country is null) return false;
            if (country.Equals(Countries.UNITED_STATES))
                return CheckOutOfCountry("US1");
            if (country.Equals(Countries.CANADA))
                return CheckOutOfCountry("Can1");
            if (country.Equals(Countries.IRELAND))
                return CheckOutOfCountry("Ire1");
            return CheckOutOfCountry("C1");
        }

        public static bool OutOfCountryCheck(CensusDate census, FactLocation location) => 
                    census is object && location is object && // checks census & location are not null
                  ((Countries.IsUnitedKingdom(census.Country) && !location.IsUnitedKingdom) ||
                  (!Countries.IsUnitedKingdom(census.Country) && census.Country != location.Country));

        public bool OutOfCountry(CensusDate census) => !(census is null) && CheckOutOfCountry(census.PropertyName);

        bool CheckOutOfCountry(string prefix)
        {
            foreach (PropertyInfo property in typeof(Individual).GetProperties())
            {
                if (property.Name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    int value = (int)property.GetValue(this, null);
                    if (value != 0 && value != 6 && value != 7)
                        return false;
                }
            }
            return true;
        }
        #endregion

        #region Colour Census Values
        public CensusColours C1841 => ColourCensusReport(CensusDate.UKCENSUS1841);

        public CensusColours C1851 => ColourCensusReport(CensusDate.UKCENSUS1851);

        public CensusColours C1861 => ColourCensusReport(CensusDate.UKCENSUS1861);

        public CensusColours C1871 => ColourCensusReport(CensusDate.UKCENSUS1871);

        public CensusColours C1881 => ColourCensusReport(CensusDate.UKCENSUS1881);

        public CensusColours C1891 => ColourCensusReport(CensusDate.UKCENSUS1891);

        public CensusColours C1901 => ColourCensusReport(CensusDate.UKCENSUS1901);

        public CensusColours C1911 => ColourCensusReport(CensusDate.UKCENSUS1911);

        public CensusColours C1921 => ColourCensusReport(CensusDate.UKCENSUS1921);

        public CensusColours C1939 => ColourCensusReport(CensusDate.UKCENSUS1939);

        public CensusColours Ire1901 => ColourCensusReport(CensusDate.IRELANDCENSUS1901);

        public CensusColours Ire1911 => ColourCensusReport(CensusDate.IRELANDCENSUS1911);

        public CensusColours US1790 => ColourCensusReport(CensusDate.USCENSUS1790);

        public CensusColours US1800 => ColourCensusReport(CensusDate.USCENSUS1800);

        public CensusColours US1810 => ColourCensusReport(CensusDate.USCENSUS1810);

        public CensusColours US1820 => ColourCensusReport(CensusDate.USCENSUS1820);

        public CensusColours US1830 => ColourCensusReport(CensusDate.USCENSUS1830);

        public CensusColours US1840 => ColourCensusReport(CensusDate.USCENSUS1840);

        public CensusColours US1850 => ColourCensusReport(CensusDate.USCENSUS1850);

        public CensusColours US1860 => ColourCensusReport(CensusDate.USCENSUS1860);

        public CensusColours US1870 => ColourCensusReport(CensusDate.USCENSUS1870);

        public CensusColours US1880 => ColourCensusReport(CensusDate.USCENSUS1880);

        public CensusColours US1890 => ColourCensusReport(CensusDate.USCENSUS1890);

        public CensusColours US1900 => ColourCensusReport(CensusDate.USCENSUS1900);

        public CensusColours US1910 => ColourCensusReport(CensusDate.USCENSUS1910);

        public CensusColours US1920 => ColourCensusReport(CensusDate.USCENSUS1920);

        public CensusColours US1930 => ColourCensusReport(CensusDate.USCENSUS1930);

        public CensusColours US1940 => ColourCensusReport(CensusDate.USCENSUS1940);

        public CensusColours US1950 => ColourCensusReport(CensusDate.USCENSUS1950);

        public CensusColours Can1851 => ColourCensusReport(CensusDate.CANADACENSUS1851);

        public CensusColours Can1861 => ColourCensusReport(CensusDate.CANADACENSUS1861);

        public CensusColours Can1871 => ColourCensusReport(CensusDate.CANADACENSUS1871);

        public CensusColours Can1881 => ColourCensusReport(CensusDate.CANADACENSUS1881);

        public CensusColours Can1891 => ColourCensusReport(CensusDate.CANADACENSUS1891);

        public CensusColours Can1901 => ColourCensusReport(CensusDate.CANADACENSUS1901);

        public CensusColours Can1906 => ColourCensusReport(CensusDate.CANADACENSUS1906);

        public CensusColours Can1911 => ColourCensusReport(CensusDate.CANADACENSUS1911);

        public CensusColours Can1916 => ColourCensusReport(CensusDate.CANADACENSUS1916);

        public CensusColours Can1921 => ColourCensusReport(CensusDate.CANADACENSUS1921);

        public CensusColours V1855 => ColourCensusReport(CensusDate.SCOTVALUATION1865);
        public CensusColours V1865 => ColourCensusReport(CensusDate.SCOTVALUATION1865);

        public CensusColours V1875 => ColourCensusReport(CensusDate.SCOTVALUATION1875);

        public CensusColours V1885 => ColourCensusReport(CensusDate.SCOTVALUATION1885);

        public CensusColours V1895 => ColourCensusReport(CensusDate.SCOTVALUATION1895);

        public CensusColours V1905 => ColourCensusReport(CensusDate.SCOTVALUATION1905);
        public CensusColours V1915 => ColourCensusReport(CensusDate.SCOTVALUATION1915);
        public CensusColours V1920 => ColourCensusReport(CensusDate.SCOTVALUATION1920);
        public CensusColours V1925 => ColourCensusReport(CensusDate.SCOTVALUATION1925);
        public CensusColours V1930 => ColourCensusReport(CensusDate.SCOTVALUATION1925);
        public CensusColours V1935 => ColourCensusReport(CensusDate.SCOTVALUATION1925);
        public CensusColours V1940 => ColourCensusReport(CensusDate.SCOTVALUATION1925);
        #endregion

        #region Colour BMD Values

        public BMDColours Birth => BirthDate.DateStatus(false);

        public BMDColours BaptChri
        {
            get
            {
                FactDate baptism = GetPreferredFactDate(Fact.BAPTISM);
                FactDate christening = GetPreferredFactDate(Fact.CHRISTENING);
                BMDColours baptismStatus = baptism.DateStatus(true);
                BMDColours christeningStatus = christening.DateStatus(true);
                if (baptismStatus.Equals(BMDColours.EMPTY))
                    return christeningStatus;
                if (christeningStatus.Equals(BMDColours.EMPTY))
                    return baptismStatus;
                return (int)baptismStatus < (int)christeningStatus ? baptismStatus : christeningStatus;
            }
        }

        BMDColours CheckMarriageStatus(Family fam)
        {
            // individual is a member of a family as parent so check family status
            if ((IndividualID == fam.HusbandID && fam.Wife is null) ||
                (IndividualID == fam.WifeID && fam.Husband is null))
            {
                return fam.Children.Count > 0 ?
                      BMDColours.NO_PARTNER // no partner but has children
                    : BMDColours.EMPTY; // solo individual so no marriage
            }
            if (fam.GetPreferredFact(Fact.MARRIAGE) is null)
                return BMDColours.NO_MARRIAGE; // has a partner but no marriage fact
            return fam.MarriageDate.DateStatus(false); // has a partner and a marriage so return date status
        }

        public BMDColours Marriage1
        {
            get
            {
                Family fam = Marriages(0);
                if (fam is null)
                {
                    if (MaxAgeAtDeath > 16 && GetPreferredFact(Fact.UNMARRIED) is null)
                        return BMDColours.NO_SPOUSE; // of marrying age but hasn't a partner or unmarried
                    return BMDColours.EMPTY;
                }
                return CheckMarriageStatus(fam);
            }
        }

        public BMDColours Marriage2
        {
            get
            {
                Family fam = Marriages(1);
                return fam is null ? BMDColours.EMPTY : CheckMarriageStatus(fam);
            }
        }

        public BMDColours Marriage3
        {
            get
            {
                Family fam = Marriages(2);
                return fam is null ? 0 : CheckMarriageStatus(fam);
            }
        }

        public string FirstMarriage => MarriageString(0);

        public string SecondMarriage => MarriageString(1);

        public string ThirdMarriage => MarriageString(2);

        public FactDate FirstMarriageDate
        {
            get
            {
                Family fam = Marriages(0);
                return fam is null ? FactDate.UNKNOWN_DATE : Marriages(0).MarriageDate;
            }
        }

        public FactDate SecondMarriageDate
        {
            get
            {
                Family fam = Marriages(1);
                return fam is null ? FactDate.UNKNOWN_DATE : Marriages(1).MarriageDate;
            }
        }

        public FactDate ThirdMarriageDate
        {
            get
            {
                Family fam = Marriages(2);
                return fam is null ? FactDate.UNKNOWN_DATE : Marriages(2).MarriageDate;
            }
        }

        public Individual FirstSpouse
        {
            get
            {
                Family fam = Marriages(0);
                return fam?.Spouse(this);
            }
        }

        public Individual SecondSpouse
        {
            get
            {
                Family fam = Marriages(1);
                return fam?.Spouse(this);
            }
        }

        public Individual ThirdSpouse
        {
            get
            {
                Family fam = Marriages(2);
                return fam?.Spouse(this);
            }
        }

        public BMDColours Death
        {
            get
            {
                if (IsFlaggedAsLiving)
                    return BMDColours.ISLIVING;
                if (DeathDate.IsUnknown && GetMaxAge(FactDate.TODAY) < FactDate.MAXYEARS)
                    return GetMaxAge(FactDate.TODAY) < 90 ? BMDColours.EMPTY : BMDColours.OVER90;
                return DeathDate.DateStatus(false);
            }
        }

        public BMDColours CremBuri
        {
            get
            {
                FactDate cremation = GetPreferredFactDate(Fact.CREMATION);
                FactDate burial = GetPreferredFactDate(Fact.BURIAL);
                BMDColours cremationStatus = cremation.DateStatus(true);
                BMDColours burialStatus = burial.DateStatus(true);
                if (cremationStatus.Equals(BMDColours.EMPTY))
                    return burialStatus;
                if (burialStatus.Equals(BMDColours.EMPTY))
                    return cremationStatus;
                return (int)cremationStatus < (int)burialStatus ? cremationStatus : burialStatus;
            }
        }

        #endregion

        public float Score
        {
            get { return 0.0f; }
            // TODO Add scoring mechanism
        }

        public int LostCousinsCensusFactCount => Facts.Count(f => f.IsLCCensusFact);

        public int CensusFactCount => Facts.Count(f => f.IsCensusFact);

        public int FactsCount => Facts.Count;

        public int SourcesCount => Facts.SelectMany(f => f.Sources).Distinct().Count();

        public int CensusDateFactCount(CensusDate censusDate) => Facts.Count(f => f.IsValidCensus(censusDate));

        public bool IsLivingError => IsFlaggedAsLiving && DeathDate.IsKnown;

        public int CensusReferenceCount(CensusReference.ReferenceStatus referenceStatus) 
            => AllFacts.Count(f => f.IsCensusFact && f.CensusReference != null && f.CensusReference.Status.Equals(referenceStatus));

        Family Marriages(int number)
        {
            if (number < FamiliesAsSpouse.Count)
            {
                Family f = FamiliesAsSpouse.OrderBy(d => d.MarriageDate).ElementAt(number);
                return f;
            }
            return null;
        }

        string MarriageString(int number)
        {
            Family marriage = Marriages(number);
            if (marriage is null)
                return string.Empty;
            if (IndividualID == marriage.HusbandID && marriage.Wife != null)
                return $"To {marriage.Wife.Name}: {marriage}";
            if (IndividualID == marriage.WifeID && marriage.Husband != null)
                return $"To {marriage.Husband.Name}: {marriage}";
            return $"Married: {marriage}";
        }

        public int NumMissingLostCousins(string country)
        {
            if (!AliveOnAnyCensus(country)) return 0;
            int numMissing = CensusDate.LOSTCOUSINS_CENSUS.Count(x => IsCensusDone(x) && !IsLostCousinsEntered(x));
            return numMissing;
        }

        #region Basic Class Functions
        public override bool Equals(object obj)
        {
            return obj is Individual individual && IndividualID == individual.IndividualID;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{IndividualID}: {Name} b.{BirthDate}";

        public int CompareTo(Individual that)
        {
            // Individuals are naturally ordered by surname, then forenames,
            // then date of birth.
            if (that is null)
                return -1;
            int res = string.Compare(Surname, that.Surname, StringComparison.CurrentCulture);
            if (res == 0)
            {
                res = string.Compare(_forenames, that._forenames, StringComparison.Ordinal);
                if (res == 0)
                {
                    FactDate d1 = BirthDate;
                    FactDate d2 = that.BirthDate;
                    res = d1.CompareTo(d2);
                }
            }
            return res;
        }

        IComparer<IDisplayIndividual> IColumnComparer<IDisplayIndividual>.GetComparer(string columnName, bool ascending)
        {
            switch(columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayIndividual>(i => i.IndividualID, ascending);
                case "Forenames": return new NameComparer<IDisplayIndividual>(ascending, true);
                case "Surname": return new NameComparer<IDisplayIndividual>(ascending, false);
                case "Gender": return CompareComparableProperty<IDisplayIndividual>(i => i.Gender, ascending);
                case "BirthDate": return CompareComparableProperty<IDisplayIndividual>(i => i.BirthDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayIndividual>(i => i.BirthLocation, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayIndividual>(i => i.DeathDate, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayIndividual>(i => i.DeathLocation, ascending);
                case "Occupation": return CompareComparableProperty<IDisplayIndividual>(i => i.Occupation, ascending);
                case "Relation": return CompareComparableProperty<IDisplayIndividual>(i => i.Relation, ascending);
                case "RelationToRoot": return CompareComparableProperty<IDisplayIndividual>(i => i.RelationToRoot, ascending);
                case "FamilySearchID": return CompareComparableProperty<IDisplayIndividual>(i => i.FamilySearchID, ascending);
                case "MarriageCount": return CompareComparableProperty<IDisplayIndividual>(i => i.MarriageCount, ascending);
                case "ChildrenCount": return CompareComparableProperty<IDisplayIndividual>(i => i.ChildrenCount, ascending);
                case "BudgieCode": return CompareComparableProperty<IDisplayIndividual>(i => i.BudgieCode, ascending);
                case "Ahnentafel": return CompareComparableProperty<IDisplayIndividual>(i => i.Ahnentafel, ascending);
                case "Notes": return CompareComparableProperty<IDisplayIndividual>(i => i.Notes, ascending);
                default: return null;
            }
        }

        IComparer<IDisplayColourBMD> IColumnComparer<IDisplayColourBMD>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayColourBMD>(i => i.IndividualID, ascending);
                case "Forenames": return new NameComparer<IDisplayColourBMD>(ascending, true);
                case "Surname": return new NameComparer<IDisplayColourBMD>(ascending, false);
                case "Relation": return CompareComparableProperty<IDisplayColourBMD>(i => i.Relation, ascending);
                case "RelationToRoot": return CompareComparableProperty<IDisplayColourBMD>(i => i.RelationToRoot, ascending);
                case "Birth": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.Birth, ascending);
                case "Baptism": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.BaptChri, ascending);
                case "Marriage 1": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.Marriage1, ascending);
                case "Marriage 2": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.Marriage2, ascending);
                case "Marriage 3": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.Marriage3, ascending);
                case "Death": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.Death, ascending);
                case "Burial": return CompareComparableProperty<IDisplayColourBMD>(i => (int)i.CremBuri, ascending);
                case "BirthDate": return CompareComparableProperty<IDisplayColourBMD>(i => i.BirthDate, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayColourBMD>(i => i.DeathDate, ascending);
                case "First Marriage": return CompareComparableProperty<IDisplayColourBMD>(i => i.FirstMarriage, ascending);
                case "Second Marriage": return CompareComparableProperty<IDisplayColourBMD>(i => i.SecondMarriage, ascending);
                case "Third Marriage": return CompareComparableProperty<IDisplayColourBMD>(i => i.ThirdMarriage, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayColourBMD>(i => i.BirthLocation, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayColourBMD>(i => i.DeathLocation, ascending);
                case "Ahnentafel": return CompareComparableProperty<IDisplayColourBMD>(i => i.Ahnentafel, ascending);
                default: return null;
            }
        }

        IComparer<IDisplayColourCensus> IColumnComparer<IDisplayColourCensus>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayColourCensus>(i => i.IndividualID, ascending);
                case "Forenames": return  new NameComparer<IDisplayColourCensus>(ascending, true);
                case "Surname": return new NameComparer<IDisplayColourCensus>(ascending, false);
                case "Relation": return CompareComparableProperty<IDisplayColourCensus>(i => i.Relation, ascending);
                case "RelationToRoot": return CompareComparableProperty<IDisplayColourCensus>(i => i.RelationToRoot, ascending);
                case "BirthDate": return CompareComparableProperty<IDisplayColourCensus>(i => i.BirthDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayColourCensus>(i => i.BirthLocation, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayColourCensus>(i => i.DeathDate, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayColourCensus>(i => i.DeathLocation, ascending);
                case "C1841": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1841, ascending);
                case "C1851": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1851, ascending);
                case "C1861": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1861, ascending);
                case "C1871": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1871, ascending);
                case "C1881": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1881, ascending);
                case "C1891": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1891, ascending);
                case "C1901": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1901, ascending);
                case "C1911": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1911, ascending);
                case "C1921": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1921, ascending);
                case "C1939": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.C1939, ascending);
                case "US1790": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1790, ascending);
                case "US1800": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1800, ascending);
                case "US1810": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1810, ascending);
                case "US1820": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1820, ascending);
                case "US1830": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1830, ascending);
                case "US1840": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1840, ascending);
                case "US1850": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1850, ascending);
                case "US1860": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1860, ascending);
                case "US1870": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1870, ascending);
                case "US1880": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1880, ascending);
                case "US1890": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1890, ascending);
                case "US1900": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1900, ascending);
                case "US1910": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1910, ascending);
                case "US1920": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1920, ascending);
                case "US1930": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1930, ascending);
                case "US1940": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1940, ascending);
                case "US1950": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.US1950, ascending);
                case "Ire1901": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Ire1901, ascending);
                case "Ire1911": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Ire1911, ascending);
                case "Can1851": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1851, ascending);
                case "Can1861": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1861, ascending);
                case "Can1871": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1871, ascending);
                case "Can1881": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1881, ascending);
                case "Can1891": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1891, ascending);
                case "Can1901": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1901, ascending);
                case "Can1906": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1906, ascending);
                case "Can1911": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1911, ascending);
                case "Can1916": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1916, ascending);
                case "Can1921": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.Can1921, ascending);
                case "V1865": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1865, ascending);
                case "V1875": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1875, ascending);
                case "V1885": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1885, ascending);
                case "V1895": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1895, ascending);
                case "V1905": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1905, ascending);
                case "V1915": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1915, ascending);
                case "V1920": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1920, ascending);
                case "V1925": return CompareComparableProperty<IDisplayColourCensus>(i => (int)i.V1925, ascending);
                case "Ahnentafel": return CompareComparableProperty<IDisplayColourCensus>(i => i.Ahnentafel, ascending);
                default: return null;
            }
        }

        IComparer<IDisplayLooseBirth> IColumnComparer<IDisplayLooseBirth>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayLooseBirth>(i => i.IndividualID, ascending);
                case "Forenames": return new NameComparer<IDisplayLooseBirth>(ascending, true);
                case "Surname": return new NameComparer<IDisplayLooseBirth>(ascending, false);
                case "BirthDate": return CompareComparableProperty<IDisplayLooseBirth>(i => i.BirthDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayLooseBirth>(i => i.BirthLocation, ascending);
                case "LooseBirth": return CompareComparableProperty<IDisplayLooseBirth>(i => i.LooseBirthDate, ascending);
                default: return null;
            }
        }

        IComparer<IDisplayLooseDeath> IColumnComparer<IDisplayLooseDeath>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayLooseDeath>(i => i.IndividualID, ascending);
                case "Forenames": return new NameComparer<IDisplayLooseDeath>(ascending, true);
                case "Surname": return new NameComparer<IDisplayLooseDeath>(ascending, false);
                case "BirthDate": return CompareComparableProperty<IDisplayLooseDeath>(i => i.DeathDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayLooseDeath>(i => i.DeathLocation, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayLooseDeath>(i => i.DeathDate, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayLooseDeath>(i => i.DeathLocation, ascending);
                case "LooseDeath": return CompareComparableProperty<IDisplayLooseDeath>(i => i.LooseDeathDate, ascending);
                default: return null;
            }
        }

        IComparer<IDisplayLooseInfo> IColumnComparer<IDisplayLooseInfo>.GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "IndividualID": return CompareComparableProperty<IDisplayLooseInfo>(i => i.IndividualID, ascending);
                case "Forenames": return new NameComparer<IDisplayLooseInfo>(ascending, true);
                case "Surname": return new NameComparer<IDisplayLooseInfo>(ascending, false);
                case "BirthDate": return CompareComparableProperty<IDisplayLooseInfo>(i => i.BirthDate, ascending);
                case "BirthLocation": return CompareComparableProperty<IDisplayLooseInfo>(i => i.BirthLocation, ascending);
                case "DeathDate": return CompareComparableProperty<IDisplayLooseInfo>(i => i.DeathDate, ascending);
                case "DeathLocation": return CompareComparableProperty<IDisplayLooseInfo>(i => i.DeathLocation, ascending);
                case "LooseBirth": return CompareComparableProperty<IDisplayLooseInfo>(i => i.LooseDeathDate, ascending);
                case "LooseDeath": return CompareComparableProperty<IDisplayLooseInfo>(i => i.LooseDeathDate, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareComparableProperty<T>(Func<Individual, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var a = accessor(x as Individual);
                var b = accessor(y as Individual);
                int result = a.CompareTo(b);
                return ascending ? result : -result;
            });
        }
        #endregion
    }
}
