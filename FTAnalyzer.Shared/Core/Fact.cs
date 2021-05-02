using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FTAnalyzer
{
    public class Fact
    {
        public const string ADOPTION = "ADOP", ADULT_CHRISTENING = "CHRA", AFN = "AFN", ALIAS = "ALIA", ANNULMENT = "ANUL",
                BAPTISM = "BAPM", BAPTISM_LDS = "BAPL", BAR_MITZVAH = "BARM", BAS_MITZVAH = "BASM", BIRTH = "BIRT",
                BIRTH_CALC = "_BIRTHCALC", BLESSING = "BLES", BURIAL = "BURI", CASTE = "CAST", CAUSE_OF_DEATH = "CAUS",
                CENSUS = "CENS", CENSUS_FTA = "_CENSFTA", CHANGE = "CHAN", CHILDREN1911 = "CHILDREN1911", CHRISTENING = "CHR",
                CIRCUMCISION = "_CIRC", CONFIRMATION = "CONF", CONFIRMATION_LDS = "CONL", CREMATION = "CREM",
                CUSTOM_ATTRIBUTE = "_ATTR", CUSTOM_EVENT = "EVEN", CUSTOM_FACT = "FACT", DEATH = "DEAT", DEGREE = "_DEG", 
                DESTINATION = "_DEST", DIVORCE = "DIV", DIVORCE_FILED = "DIVF", DNA = "_DNA", EDUCATION = "EDUC", ELECTION = "_ELEC",
                EMAIL = "EMAIL", EMIGRATION = "EMIG", EMPLOYMENT = "_EMPLOY", ENDOWMENT_LDS = "ENDL", ENGAGEMENT = "ENGA",
                EXCOMMUNICATION = "_EXCM", FIRST_COMMUNION = "FCOM", FUNERAL = "_FUN", GENDER = "SEX", GRADUATION = "GRAD",
                HEIGHT = "_HEIG", IMMIGRATION = "IMMI", INITIATORY_LDS = "_INIT", INDI = "INDI", LEGATEE = "LEGA", MARRIAGE = "MARR",
                MARRIAGE_BANN = "MARB", MARR_CONTRACT = "MARC", MARR_LICENSE = "MARL", MARR_SETTLEMENT = "MARS",
                MEDICAL_CONDITION = "_MDCL", MILITARY = "_MILT", MISSION_LDS = "_MISN", NAME = "NAME",
                NAMESAKE = "_NAMS", NATIONALITY = "NATI", NATURALIZATION = "NATU", NAT_ID_NO = "IDNO", NUM_CHILDREN = "NCHI",
                NUM_MARRIAGE = "NMR", OCCUPATION = "OCCU", ORDINATION = "ORDN", ORDINANCE = "_ORDI", ORIGIN = "_ORIG",
                PHONE = "PHON", PHYSICAL_DESC = "DSCR", PROBATE = "PROB", PROPERTY = "PROP", REFERENCE = "REFN",
                RELIGION = "RELI", RESIDENCE = "RESI", RETIREMENT = "RETI", SEALED_TO_PARENTS = "SLGC",
                SEALED_TO_SPOUSE = "SLGS", SEPARATION = "_SEPR", SERVICE_NUMBER = "_MILTID", SOCIAL_SECURITY = "SSN", TITLE = "TITL",
                UNKNOWN = "UNKN", WEIGHT = "_WEIG", WILL = "WILL", HASHTAG = "_HASHTAG", OBITUARY = "OBIT", CENSUS_SUMMARY = "CEN_SUMM";

        public const string ARRIVAL = "*ARRI", CHILDLESS = "*CHILD", CHILDREN = "*CHILDREN", CONTACT = "*CONT", DEPARTURE = "*DEPT",
                FAMILYSEARCH = "*IGI", FAMILYSEARCH_ID = "FSID", LC_FTA = "*LOST_FTA", LOOSEBIRTH = "*LOOSEB", RACE = "RACE",
                LOOSEDEATH = "*LOOSED", LOSTCOUSINS = "*LOST", MISSING = "*MISSING", PARENT = "*PARENT", REPORT = "*REPORT",
                UNMARRIED = "*UNMAR", WEBSITE = "*WEBSITE", WITNESS = "*WITNE", WORLD_EVENT = "*WORLD_EVENT";

        public const string ANCESTRY_DEATH_CAUSE = "_DCAUSE";

        public static ISet<string> LOOSE_BIRTH_FACTS = new HashSet<string>(new string[] {
            CHRISTENING, BAPTISM, RESIDENCE, WITNESS, EMIGRATION, IMMIGRATION, ARRIVAL, DEPARTURE, 
            EDUCATION, DEGREE, ADOPTION, BAR_MITZVAH, BAS_MITZVAH, ADULT_CHRISTENING, CONFIRMATION, 
            FIRST_COMMUNION, ORDINATION, NATURALIZATION, GRADUATION, RETIREMENT, LOSTCOUSINS, 
            LC_FTA, MARR_CONTRACT, MARR_LICENSE, MARR_SETTLEMENT, MARRIAGE, MARRIAGE_BANN, DEATH, 
            CREMATION, BURIAL, CENSUS, BIRTH_CALC, CENSUS_SUMMARY
                    });

        public static ISet<string> LOOSE_DEATH_FACTS = new HashSet<string>(new string[] {
            CENSUS, RESIDENCE, WITNESS, EMIGRATION, IMMIGRATION, ARRIVAL, DEPARTURE, EDUCATION,
            DEGREE, ADOPTION, BAR_MITZVAH, BAS_MITZVAH, ADULT_CHRISTENING, CONFIRMATION, FIRST_COMMUNION,
            ORDINATION, NATURALIZATION, GRADUATION, RETIREMENT, LOSTCOUSINS, LC_FTA, CENSUS_SUMMARY
                    });

        public static ISet<string> RANGED_DATE_FACTS = new HashSet<string>(new string[] {
            EDUCATION, OCCUPATION, RESIDENCE, RETIREMENT, MILITARY, ELECTION, DEGREE, EMPLOYMENT, MEDICAL_CONDITION
                    });

        public static ISet<string> IGNORE_LONG_RANGE = new HashSet<string>(new string[] {
            MARRIAGE, CHILDREN
                    });

        public static ISet<string> CREATED_FACTS = new HashSet<string>(new string[] {
            CENSUS_FTA, CHILDREN, PARENT, BIRTH_CALC, LC_FTA
                    });

        public static readonly Dictionary<string, string> NON_STANDARD_FACTS = new Dictionary<string,string>();
        static readonly Dictionary<string, string> CUSTOM_TAGS = new Dictionary<string, string>();
        static readonly HashSet<string> COMMENT_FACTS = new HashSet<string>();

        static Fact()
        {
            // special tags
            CUSTOM_TAGS.Add("IGI SEARCH", FAMILYSEARCH);
            CUSTOM_TAGS.Add("CHILDLESS", CHILDLESS);
            CUSTOM_TAGS.Add("CONTACT", CONTACT);
            CUSTOM_TAGS.Add("WITNESS", WITNESS);
            CUSTOM_TAGS.Add("WITNESSES", WITNESS);
            CUSTOM_TAGS.Add("WITN: WITNESS", WITNESS);
            CUSTOM_TAGS.Add("UNMARRIED", UNMARRIED);
            CUSTOM_TAGS.Add("FRIENDS", UNMARRIED);
            CUSTOM_TAGS.Add("PARTNERS", UNMARRIED);
            CUSTOM_TAGS.Add("UNKNOWN", UNKNOWN);
            CUSTOM_TAGS.Add("UNKNOWN-BEGIN", UNKNOWN);
            CUSTOM_TAGS.Add("ARRIVAL", ARRIVAL);
            CUSTOM_TAGS.Add("DEPARTURE", DEPARTURE);
            CUSTOM_TAGS.Add("RECORD CHANGE", CHANGE);
            CUSTOM_TAGS.Add("*CHNG", CHANGE);
            CUSTOM_TAGS.Add("LOST COUSINS", LOSTCOUSINS);
            CUSTOM_TAGS.Add("LOSTCOUSINS", LOSTCOUSINS);
            CUSTOM_TAGS.Add("DIED SINGLE", UNMARRIED);
            CUSTOM_TAGS.Add("MISSING", MISSING);
            CUSTOM_TAGS.Add("CHILDREN STATUS", CHILDREN1911);
            CUSTOM_TAGS.Add("CHILDREN1911", CHILDREN1911);
            CUSTOM_TAGS.Add("WEBSITE", WEBSITE);
            CUSTOM_TAGS.Add("_TAG1", HASHTAG);
            CUSTOM_TAGS.Add("_TAG2", HASHTAG);
            CUSTOM_TAGS.Add("_TAG3", HASHTAG);
            CUSTOM_TAGS.Add("_TAG4", HASHTAG);
            CUSTOM_TAGS.Add("_TAG5", HASHTAG);
            CUSTOM_TAGS.Add("_TAG6", HASHTAG);
            CUSTOM_TAGS.Add("_TAG7", HASHTAG);
            CUSTOM_TAGS.Add("_TAG8", HASHTAG);
            CUSTOM_TAGS.Add("_TAG9", HASHTAG);

            // convert custom tags to normal tags
            CUSTOM_TAGS.Add("CENSUS 1841", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1851", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1861", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1871", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1881", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1891", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1901", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1911", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1939", CENSUS);
            CUSTOM_TAGS.Add("1841 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1851 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1861 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1871 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1881 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1891 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1901 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1911 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1939 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("NAT REG 1939", CENSUS);
            CUSTOM_TAGS.Add("REGISTER 1939", CENSUS);
            CUSTOM_TAGS.Add("NAT REGISTER 1939", CENSUS);
            CUSTOM_TAGS.Add("NATIONAL REGISTER 1939", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1841", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1851", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1861", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1871", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1881", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1891", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1901", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1911", CENSUS);
            CUSTOM_TAGS.Add("CENSUS UK 1939", CENSUS);
            CUSTOM_TAGS.Add("UK 1841 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1851 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1861 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1871 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1881 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1891 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1901 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1911 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1921 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("UK 1939 CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1841 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1851 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1861 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1871 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1881 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1891 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1901 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1911 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1921 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1939 UK CENSUS", CENSUS);
            CUSTOM_TAGS.Add("1939 UK REGISTER", CENSUS);
            CUSTOM_TAGS.Add("REGISTER UK 1939", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1790", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1800", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1810", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1820", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1830", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1840", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1850", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1860", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1870", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1880", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1890", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1900", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1910", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1920", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1930", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1940", CENSUS);
            CUSTOM_TAGS.Add("CENSUS 1950", CENSUS);
            CUSTOM_TAGS.Add("CENSUS - US", CENSUS);
            CUSTOM_TAGS.Add("CENSUS - STATE", CENSUS);
            CUSTOM_TAGS.Add("CENSUS- US", CENSUS);
            CUSTOM_TAGS.Add("CENSUS- STATE", CENSUS);
            CUSTOM_TAGS.Add("BAPTISED", BAPTISM);
            CUSTOM_TAGS.Add("BIRTH REG", BIRTH);
            CUSTOM_TAGS.Add("BIRTH", BIRTH);
            CUSTOM_TAGS.Add("ALTERNATE BIRTH", BIRTH);
            CUSTOM_TAGS.Add("ALTERNATIVE BIRTH", BIRTH);
            CUSTOM_TAGS.Add("BIRTH CERTIFICATE", BIRTH);
            CUSTOM_TAGS.Add("BIRTH CERT", BIRTH);
            CUSTOM_TAGS.Add("MARRIAGE REG", MARRIAGE);
            CUSTOM_TAGS.Add("MARRIAGE", MARRIAGE);
            CUSTOM_TAGS.Add("MARRIAGE CERTIFICATE", MARRIAGE);
            CUSTOM_TAGS.Add("MARRIAGE CERT", MARRIAGE);
            CUSTOM_TAGS.Add("ALTERNATE MARRIAGE", BIRTH);
            CUSTOM_TAGS.Add("ALTERNATIVE MARRIAGE", BIRTH);
            CUSTOM_TAGS.Add("SAME SEX MARRIAGE", MARRIAGE);
            CUSTOM_TAGS.Add("CIVIL", MARRIAGE);
            CUSTOM_TAGS.Add("CIVIL PARTNER", MARRIAGE);
            CUSTOM_TAGS.Add("CIVIL PARTNERSHIP", MARRIAGE);
            CUSTOM_TAGS.Add("DEATH REG", DEATH);
            CUSTOM_TAGS.Add("DEATH", DEATH);
            CUSTOM_TAGS.Add("ALTERNATE DEATH", DEATH);
            CUSTOM_TAGS.Add("ALTERNATIVE DEATH", DEATH);
            CUSTOM_TAGS.Add("DEATH CERTIFICATE", DEATH);
            CUSTOM_TAGS.Add("DEATH CERT", DEATH);
            CUSTOM_TAGS.Add("DEATH NOTICE", DEATH);
            CUSTOM_TAGS.Add("DEATH NOTICE REF", DEATH);
            CUSTOM_TAGS.Add("CHRISTENING", CHRISTENING);
            CUSTOM_TAGS.Add("CHRISTENED", CHRISTENING);
            CUSTOM_TAGS.Add("BURIAL", BURIAL);
            CUSTOM_TAGS.Add("FUNERAL", BURIAL);
            CUSTOM_TAGS.Add("CREMATION", CREMATION);
            CUSTOM_TAGS.Add("CREMATED", CREMATION);
            CUSTOM_TAGS.Add("PROBATE", PROBATE);
            CUSTOM_TAGS.Add("GRANT OF PROBATE", PROBATE);
            CUSTOM_TAGS.Add("PROBATE DATE", PROBATE);
            CUSTOM_TAGS.Add("RESIDENCE", RESIDENCE);
            CUSTOM_TAGS.Add("DIVORCED", DIVORCE);
            CUSTOM_TAGS.Add("CENSUS", CENSUS);
            CUSTOM_TAGS.Add("OCCUPATION", OCCUPATION);
            CUSTOM_TAGS.Add("NATURALIZATION", NATURALIZATION);
            CUSTOM_TAGS.Add("NATURALISATION", NATURALIZATION);
            CUSTOM_TAGS.Add("CURRENT RESIDENCE", RESIDENCE);
            CUSTOM_TAGS.Add("HIGH SCHOOL", EDUCATION);
            CUSTOM_TAGS.Add("COLLEGE", EDUCATION);
            CUSTOM_TAGS.Add("TERTIARY EDUCATION", EDUCATION);
            CUSTOM_TAGS.Add("UNIVERSITY", EDUCATION);
            CUSTOM_TAGS.Add("DIPLOMA", EDUCATION);
            CUSTOM_TAGS.Add("SCHL: SCHOOL ATTENDANCE", EDUCATION);
            CUSTOM_TAGS.Add("EMPL: EMPLOYMENT", EMPLOYMENT);
            CUSTOM_TAGS.Add("MARL: MARRIAGE LICENCE", MARR_LICENSE);
            CUSTOM_TAGS.Add("FUNL: FUNERAL", FUNERAL);
            CUSTOM_TAGS.Add("CAUSE OF DEATH (FACTS PAGE)", CAUSE_OF_DEATH);
            CUSTOM_TAGS.Add("LTOG: LIVED TOGETHER (UNMARRIED)", UNMARRIED);
            CUSTOM_TAGS.Add("ILLNESS", MEDICAL_CONDITION);
            CUSTOM_TAGS.Add("CENSUS SUMMARY", CENSUS_SUMMARY);
            
            // Legacy 8 default fact types
            CUSTOM_TAGS.Add("ALT. BIRTH", BIRTH);
            CUSTOM_TAGS.Add("ALT. CHRISTENING", CHRISTENING);
            CUSTOM_TAGS.Add("ALT. DEATH", DEATH);
            CUSTOM_TAGS.Add("ALT. BURIAL", BURIAL);
            CUSTOM_TAGS.Add("ALT. MARRIAGE", MARRIAGE);
            CUSTOM_TAGS.Add("DIVORCE FILING", DIVORCE_FILED);
            CUSTOM_TAGS.Add("DEGREE", DEGREE);
            CUSTOM_TAGS.Add("ELECTION", ELECTION);
            CUSTOM_TAGS.Add("EMPLOYMENT", EMPLOYMENT);
            CUSTOM_TAGS.Add("MARRIAGE LICENCE", MARR_LICENSE);
            CUSTOM_TAGS.Add("MARRIAGE LICENSE", MARR_LICENSE);
            CUSTOM_TAGS.Add("MARRIAGE CONTRACT", MARR_CONTRACT);
            CUSTOM_TAGS.Add("MEDICAL", MEDICAL_CONDITION);
            CUSTOM_TAGS.Add("MILITARY", MILITARY);
            CUSTOM_TAGS.Add("MILITARY SERVICE", MILITARY);
            CUSTOM_TAGS.Add("MILITARY ENLISTMENT", MILITARY);
            CUSTOM_TAGS.Add("MILITARY DISCHARGE", MILITARY);
            CUSTOM_TAGS.Add("MILITARY AWARD", MILITARY);
            CUSTOM_TAGS.Add("PROPERTY", PROPERTY);

            // Convert non standard fact types to standard ones
            NON_STANDARD_FACTS.Add(ANCESTRY_DEATH_CAUSE, CAUSE_OF_DEATH);

            // Create list of Comment facts treat text as comment rather than location
            COMMENT_FACTS.Add(AFN);
            COMMENT_FACTS.Add(ALIAS);
            COMMENT_FACTS.Add(CASTE);
            COMMENT_FACTS.Add(CAUSE_OF_DEATH);
            COMMENT_FACTS.Add(CHILDLESS);
            COMMENT_FACTS.Add(CHILDREN);
            COMMENT_FACTS.Add(CHILDREN1911);
            COMMENT_FACTS.Add(DESTINATION);
            COMMENT_FACTS.Add(FAMILYSEARCH);
            COMMENT_FACTS.Add(HASHTAG);
            COMMENT_FACTS.Add(HEIGHT);
            COMMENT_FACTS.Add(MISSING);
            COMMENT_FACTS.Add(NAME);
            COMMENT_FACTS.Add(NAMESAKE);
            COMMENT_FACTS.Add(NATIONALITY);
            COMMENT_FACTS.Add(OBITUARY);
            COMMENT_FACTS.Add(PARENT);
            COMMENT_FACTS.Add(RACE);
            COMMENT_FACTS.Add(REFERENCE);
            COMMENT_FACTS.Add(RELIGION);
            COMMENT_FACTS.Add(SOCIAL_SECURITY);
            COMMENT_FACTS.Add(TITLE);
            COMMENT_FACTS.Add(UNKNOWN);
            COMMENT_FACTS.Add(UNMARRIED);
            COMMENT_FACTS.Add(WEIGHT);
            COMMENT_FACTS.Add(WILL);
            COMMENT_FACTS.Add(WITNESS);
        }

        internal static string GetFactTypeDescription(string factType)
        {
            switch (factType)
            {
                case ADOPTION: return "Adoption";
                case ADULT_CHRISTENING: return "Adult christening";
                case AFN: return "Ancestral File Number";
                case ALIAS: return "Also known as";
                case ANNULMENT: return "Annulment";
                case ARRIVAL: return "Arrival";
                case BAPTISM: return "Baptism";
                case BAPTISM_LDS: return "Baptism (LDS)";
                case BAR_MITZVAH: return "Bar mitzvah";
                case BAS_MITZVAH: return "Bas mitzvah";
                case BIRTH: return "Birth";
                case BIRTH_CALC: return "Birth (Calc from Age)";
                case BLESSING: return "Blessing";
                case BURIAL: return "Burial";
                case CASTE: return "Caste";
                case CAUSE_OF_DEATH: return "Cause of Death";
                case CENSUS: return "Census";
                case CENSUS_FTA: return "Census (FTAnalyzer)";
                case CENSUS_SUMMARY: return "Census Summary";
                case CHANGE: return "Record change";
                case CHILDLESS: return "Childless";
                case CHILDREN1911: return "Children Status";
                case CHILDREN: return "Child Born";
                case CHRISTENING: return "Christening";
                case CIRCUMCISION: return "Circumcision";
                case CONFIRMATION: return "Confirmation";
                case CONFIRMATION_LDS: return "Confirmation (LDS)";
                case CONTACT: return "Contact";
                case CREMATION: return "Cremation";
                case CUSTOM_ATTRIBUTE: return "Custom Attribute";
                case CUSTOM_EVENT: return "Event";
                case CUSTOM_FACT: return "Custom Fact";
                case DEATH: return "Death";
                case DEGREE: return "Degree";
                case DEPARTURE: return "Departure";
                case DESTINATION: return "Destination";
                case DIVORCE: return "Divorce";
                case DIVORCE_FILED: return "Divorce filed";
                case DNA: return "DNA Markers";
                case EDUCATION: return "Education";
                case ELECTION: return "Election";
                case EMAIL: return "Email Address";
                case EMIGRATION: return "Emigration";
                case EMPLOYMENT: return "Employment";
                case ENDOWMENT_LDS: return "Endowment (LDS)";
                case ENGAGEMENT: return "Engagement";
                case EXCOMMUNICATION: return "Excommunication";
                case FAMILYSEARCH: return "FamilySearch";
                case FAMILYSEARCH_ID: return "FamilySearch ID";
                case FIRST_COMMUNION: return "First communion";
                case FUNERAL: return "Funeral";
                case GENDER: return "Gender";
                case GRADUATION: return "Graduation";
                case HASHTAG: return "Hashtag";
                case HEIGHT: return "Height";
                case IMMIGRATION: return "Immigration";
                case INDI: return "Name";
                case INITIATORY_LDS: return "Initiatory (LDS)";
                case LC_FTA: return "Lost Cousins (FTAnalyzer)";
                case LEGATEE: return "Legatee";
                case LOOSEBIRTH: return "Loose birth";
                case LOOSEDEATH: return "Loose death";
                case LOSTCOUSINS: return "Lost Cousins";
                case MARRIAGE: return "Marriage";
                case MARRIAGE_BANN: return "Marriage banns";
                case MARR_CONTRACT: return "Marriage contract";
                case MARR_LICENSE: return "Marriage license";
                case MARR_SETTLEMENT: return "Marriage settlement";
                case MEDICAL_CONDITION: return "Medical condition";
                case MILITARY: return "Military service";
                case MISSING: return "Missing";
                case MISSION_LDS: return "Mission (LDS)";
                case NAME: return "Alternate Name";
                case NAMESAKE: return "Namesake";
                case NATIONALITY: return "Nationality";
                case NATURALIZATION: return "Naturalization";
                case NAT_ID_NO: return "National identity no.";
                case NUM_CHILDREN: return "Number of children";
                case NUM_MARRIAGE: return "Number of marriages";
                case OBITUARY: return "Obituary";
                case OCCUPATION: return "Occupation";
                case ORDINATION: return "Ordination";
                case ORDINANCE: return "Ordinance";
                case PARENT: return "Parental Info";
                case PHONE: return "Phone";
                case PHYSICAL_DESC: return "Physical description";
                case PROBATE: return "Probate";
                case PROPERTY: return "Property";
                case RACE: return "Race";
                case REFERENCE: return "Reference ID";
                case RELIGION: return "Religion";
                case REPORT: return "Fact Report";
                case RESIDENCE: return "Residence";
                case RETIREMENT: return "Retirement";
                case SEALED_TO_PARENTS: return "Sealed to Parents (LDS)";
                case SEALED_TO_SPOUSE: return "Sealed to Spouse (LDS)";
                case SEPARATION: return "Separation";
                case SERVICE_NUMBER: return "Military service number";
                case SOCIAL_SECURITY: return "Social Security number";
                case TITLE: return "Title";
                case UNKNOWN: return "UNKNOWN";
                case UNMARRIED: return "Unmarried";
                case WEIGHT: return "Weight";
                case WILL: return "Will";
                case WITNESS: return "Witness";
                case WORLD_EVENT: return "World Event";
                case "": return "UNKNOWN";
                default: return EnhancedTextInfo.ToTitleCase(factType);
            }
        }

        public enum FactError { GOOD = 0, WARNINGALLOW = 1, WARNINGIGNORE = 2, ERROR = 3, QUESTIONABLE = 4, IGNORE = 5 };

        #region Constructors

        Fact(string reference, bool preferred)
        {
            FactType = string.Empty;
            FactDate = FactDate.UNKNOWN_DATE;
            Comment = string.Empty;
            Place = string.Empty;
            Location = FactLocation.BLANK_LOCATION;
            Sources = new List<FactSource>();
            CensusReference = CensusReference.UNKNOWN;
            CertificatePresent = false;
            FactErrorLevel = FactError.GOOD;
            FactErrorMessage = string.Empty;
            FactErrorNumber = 0;
            GedcomAge = null;
            Created = false;
            Tag = string.Empty;
            Preferred = preferred;
            Reference = reference;
            SourcePages = new List<string>();
        }

        public Fact(XmlNode node, Family family, bool preferred, IProgress<string> outputText)
            : this(family.FamilyRef, preferred)
        {
            Individual = null;
            Family = family;
            CreateFact(node, family.FamilyRef, preferred, null, outputText);
        }

        public Fact(XmlNode node, Individual ind, bool preferred, FactDate deathdate, IProgress<string> outputText)
            : this(ind.IndividualID, preferred)
        {
            Individual = ind;
            Family = null;
            CreateFact(node, ind.IndividualRef, preferred, deathdate, outputText);
        }

        void CreateFact(XmlNode node, string reference, bool preferred, FactDate deathdate, IProgress<string> outputText)
        {
            if (node != null)
            {
                FamilyTree ft = FamilyTree.Instance;
                try
                {
                    FactType = FixFactTypes(node.Name);
                    if (deathdate != null)
                        FactDate = deathdate;
                    else
                    {
                        string factDate = FamilyTree.GetText(node, "DATE", false);
                        try
                        {
                            FactDate = new FactDate(factDate, reference);
                        }
                        catch (FactDateException e)
                        {
                            outputText.Report(e.Message);
                        }
                    }
                    Preferred = preferred;
                    if (FactType.Equals(CUSTOM_ATTRIBUTE) || FactType.Equals(CUSTOM_EVENT) || FactType.Equals(CUSTOM_FACT))
                    {
                        string tag = FamilyTree.GetText(node, "TYPE", false).ToUpper();
                        if(tag.StartsWith("CENSUS") || tag.StartsWith("1939 REGISTER"))
                        {
                            FactType = CENSUS;
                            CheckCensusDate(tag);
                        }
                        else if (CUSTOM_TAGS.TryGetValue(tag, out string factType))
                        {
                            FactType = factType;
                            CheckCensusDate(tag);
                        }
                        else
                        {
                            FactType = UNKNOWN;
                            FamilyTree.Instance.CheckUnknownFactTypes(tag);
                            Tag = string.IsNullOrEmpty(tag) ? "** Custom Fact with no Fact Type ERROR **" : tag;
                        }
                    }
                    if(FactType.Equals(NAME))
                    {
                        string tag = FamilyTree.GetText(node, "TYPE", false).ToUpper();
                        if (tag.Equals("AKA"))
                            FactType = ALIAS;
                    }
                    var nodeText = FamilyTree.GetText(node, false);
                    var placeText = FamilyTree.GetText(node, "PLAC", false);
                    var xmlLat = FamilyTree.GetText(node, "PLAC/MAP/LATI", false);
                    var xmlLong = FamilyTree.GetText(node, "PLAC/MAP/LONG", false);
                    var addrTagText = GetAddress(FactType, node);
                    SetCommentAndLocation(FactType, nodeText, placeText, addrTagText, xmlLat, xmlLong);
                    if (!string.IsNullOrEmpty(xmlLat) && !string.IsNullOrEmpty(xmlLong))
                        Location.GEDCOMLatLong = true;
                    
                    // only check UK census dates for errors as those are used for colour census
                    if (FactType.Equals(CENSUS) && Location.IsUnitedKingdom)
                        CheckCensusDate("Census");

                    // need to check residence after setting location
                    if (FactType.Equals(RESIDENCE) && GeneralSettings.Default.UseResidenceAsCensus)
                        CheckResidenceCensusDate();

                    // check Children Status is valid
                    if (FactType.Equals(CHILDREN1911))
                        CheckValidChildrenStatus(node);

                    // now iterate through source elements of the fact finding all sources
                    XmlNodeList list = node.SelectNodes("SOUR");
                    foreach (XmlNode n in list)
                    {
                        if (n.Attributes["REF"] != null)
                        {   // only process sources with a reference
                            string srcref = n.Attributes["REF"].Value;
                            FactSource source = ft.GetSource(srcref);
                            string pageText = FamilyTree.GetText(n, "PAGE", true); // Source page text
                            if (source != null)
                            {
                                Sources.Add(source);
                                source.AddFact(this);
                                if(!SourcePages.Contains(pageText))
                                    SourcePages.Add(pageText);
                            }
                            else
                                outputText.Report($"Source {srcref} not found.\n");
                            if (IsCensusFact)
                                CensusReference = new CensusReference(this, n);
                        }
                    }
                    // if we have checked the sources and no census ref see if its been added as a comment to this fact
                    if (IsCensusFact)
                    {
                        CheckForSharedFacts(node);

                        if (!CensusReference.IsKnownStatus)
                            CensusReference = new CensusReference(this, node, CensusReference); //pass in existing reference so as to not lose any unknown references
                        else if (!CensusReference.IsGoodStatus)
                            CensusReference.CheckFullUnknownReference(CensusReference.Status);
                    }
                    if(GeneralSettings.Default.ConvertResidenceFacts && FactType.Equals(RESIDENCE) && CensusReference.IsKnownStatus)
                            FactType = CENSUS; // change fact type if option set and residence has a valid census reference
                    if (FactType == DEATH)
                    {
                        Comment = FamilyTree.GetText(node, "CAUS", true);
                        if (node.FirstChild != null && node.FirstChild.Value == "Y" && FactDate.IsUnknown)
                            FactDate = new FactDate(FactDate.MINDATE, FactDate.NOW); // if death flag set as Y then death before today.
                    }
                    string age = FamilyTree.GetText(node, "AGE", false);
                    if (age.Length > 0)
                        GedcomAge = new Age(age, FactDate);
                    CertificatePresent = SetCertificatePresent();
                    if (FactDate.SpecialDate)
                    {
                        //if (FactType == DEATH || FactType == MARRIAGE)
                        //    throw;
                        //string message = (node is null) ? string.Empty : node.InnerText + ". ";
                        //throw new InvalidXMLFactException($"{message}\n            Error {te.Message} text in {FactTypeDescription} fact - a non death fact.\n");
                    }
                }
                catch (Exception ex)
                {
                    string message = (node is null) ? string.Empty : node.InnerText + ". ";
                    throw new InvalidXMLFactException($"{message}\n            Error {ex.Message} in {FactTypeDescription} fact\n");
                }
            }
        }

        void CheckForSharedFacts(XmlNode node)
        {
            XmlNodeList list = node.SelectNodes("_SHAR");
            foreach (XmlNode n in list)
            {
                string indref = n.Attributes["REF"]?.Value;
                string role = FamilyTree.GetText(n, "ROLE", false);
                if (indref != null && role == "Household Member")
                    FamilyTree.Instance.AddSharedFact(indref, this);
            }
        }

        const string CHILDREN_STATUS_PATTERN1 = @"(\d{1,2}) Total ?,? ?(\d{1,2}) (Alive|Living) ?,? ?(\d{1,2}) Dead";
        const string CHILDREN_STATUS_PATTERN2 = @"Total:? (\d{1,2}) ?,? ?(Alive|Living):? (\d{1,2}) ?,? ?Dead:? (\d{1,2})";
        public readonly static Regex regexChildren1 = new Regex(CHILDREN_STATUS_PATTERN1, RegexOptions.Compiled);
        public readonly static Regex regexChildren2 = new Regex(CHILDREN_STATUS_PATTERN2, RegexOptions.Compiled);

        void CheckValidChildrenStatus(XmlNode node)
        {
            if (Comment.Length == 0)
                Comment = FamilyTree.GetNotes(node);
            if (Comment.IndexOf("ignore", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                FactErrorLevel = FactError.IGNORE;
                return;
            }
            bool success = false;
            int total, alive, dead;
            total = alive = dead = 0;
            Match matcher = regexChildren1.Match(Comment);
            if (matcher.Success)
            {
                success = true;
                int.TryParse(matcher.Groups[1].ToString(), out total);
                int.TryParse(matcher.Groups[2].ToString(), out alive);
                int.TryParse(matcher.Groups[4].ToString(), out dead);
            }
            else
            {
                matcher = regexChildren2.Match(Comment);
                if (matcher.Success)
                {
                    success = true;
                    int.TryParse(matcher.Groups[1].ToString(), out total);
                    int.TryParse(matcher.Groups[3].ToString(), out alive);
                    int.TryParse(matcher.Groups[4].ToString(), out dead);
                }
            }
            if (success)
            {
                if (total == alive + dead)
                    return;
                FactErrorMessage = "Children status total doesn't equal numbers alive plus numbers dead.";
            }
            else
                FactErrorMessage = "Children status doesn't match valid pattern Total x, Alive y, Dead z";
            FactErrorNumber = (int)FamilyTree.Dataerror.CHILDRENSTATUS_TOTAL_MISMATCH;
            FactErrorLevel = FactError.ERROR;
        }

        string GetAddress(string factType, XmlNode node)
        {
            XmlNode addr = node.SelectSingleNode("ADDR");
            if (addr is null)
                return string.Empty;
            if (COMMENT_FACTS.Contains(factType)) // don't parse address records if this is a comment fact
                return string.Empty;
            string result = string.Empty; // need to do something with an ADDR tag
            XmlNode ctry = node.SelectSingleNode("ADDR/CTRY");
            if (ctry != null)
                result = (result.Length > 0) ? ctry.InnerText + ", " + result : ctry.InnerText;
            XmlNode stae = node.SelectSingleNode("ADDR/STAE");
            if (stae != null)
                result = (result.Length > 0) ? stae.InnerText + ", " + result : stae.InnerText;
            XmlNode city = node.SelectSingleNode("ADDR/CITY");
            if (city != null)
                result = (result.Length > 0) ? city.InnerText + ", " + result : city.InnerText;
            XmlNode adr3 = node.SelectSingleNode("ADDR/ADR3");
            if (adr3 != null)
                result = (result.Length > 0) ? adr3.InnerText + ", " + result : adr3.InnerText;
            XmlNode adr2 = node.SelectSingleNode("ADDR/ADR2");
            if (adr2 != null)
                result = (result.Length > 0) ? adr2.InnerText + ", " + result : adr2.InnerText;
            XmlNode adr1 = node.SelectSingleNode("ADDR/ADR1");
            if (adr1 != null)
                result = (result.Length > 0) ? adr1.InnerText + ", " + result : adr1.InnerText;
            string address = string.Empty;
            if (addr.FirstChild != null && addr.FirstChild.Value != null)
                address = addr.FirstChild.Value;
            XmlNodeList list = node.SelectNodes("ADDR/CONT");
            foreach (XmlNode cont in list)
            {
                if (cont.FirstChild != null && cont.FirstChild.Value != null)
                    address += " " + cont.FirstChild.Value;
            }
            if (address.Length > 0)
                result = (result.Length > 0) ? address + ", " + result : address;
            //   ADDR <ADDRESS_LINE> {1:1} p.41
            //+1 CONT <ADDRESS_LINE> {0:3} p.41
            //+1 ADR1 <ADDRESS_LINE1> {0:1} p.41
            //+1 ADR2 <ADDRESS_LINE2> {0:1} p.41
            //+1 ADR3 <ADDRESS_LINE3> {0:1} p.41
            //+1 CITY <ADDRESS_CITY> {0:1} p.41
            //+1 STAE <ADDRESS_STATE> {0:1} p.42
            //+1 POST <ADDRESS_POSTAL_CODE> {0:1} p.41
            //+1 CTRY <ADDRESS_COUNTRY> 
            return result;
        }

        public Fact(string factRef, string factType, FactDate date, FactLocation loc, string comment = "", bool preferred = true, bool createdByFTA = false, Individual ind = null)
            : this(factRef, preferred)
        {
            FactType = factType;
            FactDate = date ?? FactDate.UNKNOWN_DATE;
            Comment = comment;
            Created = createdByFTA;
            Place = string.Empty;
            Location = loc;
            Individual = ind;
        }

        #endregion

        #region Properties

        string Reference { get; set; }
        string Tag { get; set; }
        public Age GedcomAge { get; private set; }
        public bool Created { get; protected set; }
        public bool Preferred { get; private set; }
        public CensusReference CensusReference { get; private set; }
        public FactLocation Location { get; private set; }
        public string Place { get; private set; }
        public string Comment { get; set; }
        public FactDate FactDate { get; private set; }
        public string FactType { get; private set; }
        public int FactErrorNumber { get; private set; }
        public FactError FactErrorLevel { get; private set; }
        public string FactErrorMessage { get; private set; }
        public Individual Individual { get; private set; }
        public Family Family { get; private set; }
        public string FactTypeDescription => (FactType == UNKNOWN && Tag.Length > 0) ? Tag : GetFactTypeDescription(FactType);

        public bool IsMarriageFact =>  
            FactType == MARR_CONTRACT || FactType == MARR_LICENSE || 
            FactType == MARR_SETTLEMENT || FactType == MARRIAGE || FactType == MARRIAGE_BANN;

        public bool IsCensusFact
        {
            get
            {
                if (FactType == CENSUS || FactType == CENSUS_FTA) return true;
                if (FactType == RESIDENCE && GeneralSettings.Default.UseResidenceAsCensus)
                    return CensusDate.IsCensusYear(FactDate, Country, GeneralSettings.Default.TolerateInaccurateCensusDate);
                return false;
            }
        }

        public bool IsLCCensusFact
        {
            get
            {
                if (!IsCensusFact)
                    return false;
                if (!CensusDate.IsLostCousinsCensusYear(FactDate, false))
                    return false;
                if (FactDate.CensusYearMatches(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(Country))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(Country))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.SCOTCENSUS1881) && Country.Equals(Countries.SCOTLAND))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.CANADACENSUS1881) && Country.Equals(Countries.CANADA))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(Country))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.IRELANDCENSUS1911) && Country.Equals(Countries.IRELAND))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.USCENSUS1880) && Country.Equals(Countries.UNITED_STATES))
                    return true;
                if (FactDate.CensusYearMatches(CensusDate.USCENSUS1940) && Country.Equals(Countries.UNITED_STATES))
                    return true;
                return false;
            }
        }

        public static bool IsAlwaysLoadFact(string factType)
        {
            if (factType == CENSUS || factType == CENSUS_FTA) return true;
            if (factType == RESIDENCE && GeneralSettings.Default.UseResidenceAsCensus) return true;
            if (factType == LOSTCOUSINS || factType == LC_FTA || factType == CUSTOM_EVENT || factType == CUSTOM_FACT) return true;
            return false;
        }

        public string DateString
        {
            get { return FactDate is null ? string.Empty : FactDate.DateString; }
        }

        public string SourceList
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (FactSource s in Sources.OrderBy(s => s.ToString()))
                {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(s.ToString());
                }
                return sb.ToString();
            }
        }

        public IList<FactSource> Sources { get; private set; }

        public int SourcesCount => Sources.Count;

        public List<string> SourcePages { get; private set; }

        public string Country => Location is null ? "UNKNOWN" : Location.Country;

        public bool CertificatePresent { get; private set; }

        #endregion

        public static string ReverseLocation(string location) => string.Join(",", location.Split(',').Reverse());

        public void ChangeNonStandardFactType(string factType) => FactType = factType;

        public void SetError(int number, FactError level, string message)
        {
            FactErrorNumber = number;
            FactErrorLevel = level;
            FactErrorMessage = message;
        }

        string FixFactTypes(string tag)
        {
            string initialChars = tag.ToUpper().Substring(0, Math.Min(tag.Length, 4));
            if (initialChars == "BIRT" || initialChars == "MARR" || initialChars == "DEAT")
                return initialChars;
            return tag;
        }

        public void UpdateFactDate(FactDate date)
        {
            if (FactDate.IsUnknown && date != null && date.IsKnown)
                FactDate = date;
        }

        public void SetCensusReferenceDetails(CensusReference cr, CensusLocation cl, string comment)
        {
            if (cr is null || cl is null) return;
            comment = comment ?? string.Empty;
            if ((cr.IsKnownStatus && !CensusReference.IsKnownStatus) || (cr.IsGoodStatus && !CensusReference.IsGoodStatus))
                CensusReference = cr;
            if (Location.IsBlank || !Location.IsKnown)
                Location = cl.Equals(CensusLocation.UNKNOWN) ?
                    FactLocation.GetLocation(cr.Country, GeneralSettings.Default.AddCreatedLocations) :
                    FactLocation.GetLocation(cl.Location, GeneralSettings.Default.AddCreatedLocations);
            if (Comment.Length == 0 && comment.Length > 0)
                Comment = comment;
            if (cr.Class == "RG78")
                FactType = CENSUS_SUMMARY;
        }

        void CheckResidenceCensusDate()
        {
            if (FactDate.IsKnown && CensusDate.IsCensusYear(FactDate, Country, true) && !CensusDate.IsCensusYear(FactDate, Country, false))
            {
                // residence isn't a normal census year but it is a census year if tolerate is on
                if (CensusDate.IsCensusCountry(FactDate, Location) || !Location.IsKnownCountry)
                {
                    //                    FactErrorNumber = (int) FamilyTree.Dataerror.RESIDENCE_CENSUS_DATE;
                    FactErrorLevel = FactError.WARNINGALLOW;
                    FactErrorMessage = $"Warning : Residence date {FactDate} is in a census year but doesn't overlap census date.";
                    if (!GeneralSettings.Default.TolerateInaccurateCensusDate)
                        FactErrorMessage += " This would be accepted as a census fact with Tolerate slightly inaccurate census dates option.";
                }
            }
        }

        void CheckCensusDate(string tag)
        {
            FactDate yearAdjusted = FactDate;
            // check if census fails to overlaps a census date
            if ((tag == "Census 1841" && !FactDate.Overlaps(CensusDate.UKCENSUS1841)) ||
                (tag == "Census 1851" && !FactDate.Overlaps(CensusDate.UKCENSUS1851)) ||
                (tag == "Census 1861" && !FactDate.Overlaps(CensusDate.UKCENSUS1861)) ||
                (tag == "Census 1871" && !FactDate.Overlaps(CensusDate.UKCENSUS1871)) ||
                (tag == "Census 1881" && !FactDate.Overlaps(CensusDate.UKCENSUS1881)) ||
                (tag == "Census 1891" && !FactDate.Overlaps(CensusDate.UKCENSUS1891)) ||
                (tag == "Census 1901" && !FactDate.Overlaps(CensusDate.UKCENSUS1901)) ||
                (tag == "Census 1911" && !FactDate.Overlaps(CensusDate.UKCENSUS1911)) ||
                (tag == "Census 1921" && !FactDate.Overlaps(CensusDate.UKCENSUS1921)) ||
                (tag == "Census 1939" && !FactDate.Overlaps(CensusDate.UKCENSUS1939)) ||
                (tag == "Census" && !CensusDate.IsUKCensusYear(FactDate, false)) ||
                ((tag == "Lost Cousins" || tag == "LostCousins") && !CensusDate.IsLostCousinsCensusYear(FactDate, false))
                && FactDate.DateString.Length >= 4)
            {
                // if not a census overlay then set date to year and try that instead
                string year = FactDate.DateString.Substring(FactDate.DateString.Length - 4);
                if (int.TryParse(year, out int result))
                {
                    yearAdjusted = new FactDate(year);
                    if (GeneralSettings.Default.TolerateInaccurateCensusDate)
                    {
                        //                        FactErrorNumber = (int)FamilyTree.Dataerror.RESIDENCE_CENSUS_DATE;
                        FactErrorMessage = $"Warning: Inaccurate Census date '{FactDate}' treated as '{yearAdjusted}'";
                        FactErrorLevel = FactError.WARNINGALLOW;
                    }
                    else
                    {
                        //                        FactErrorNumber = (int)FamilyTree.Dataerror.RESIDENCE_CENSUS_DATE;
                        FactErrorLevel = FactError.WARNINGIGNORE;
                        FactErrorMessage = $"Inaccurate Census date '{FactDate}' fact ignored in strict mode. Check for incorrect date entered or try Tolerate slightly inaccurate census date option.";
                    }
                }
            }
            if ((tag == "Census 1841" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1841)) ||
                (tag == "Census 1851" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1851)) ||
                (tag == "Census 1861" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1861)) ||
                (tag == "Census 1871" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1871)) ||
                (tag == "Census 1881" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1881)) ||
                (tag == "Census 1891" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1891)) ||
                (tag == "Census 1901" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1901)) ||
                (tag == "Census 1911" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1911)) ||
                (tag == "Census 1939" && !yearAdjusted.Overlaps(CensusDate.UKCENSUS1939)))
            {
                FactErrorMessage = $"UK Census fact error date '{FactDate}' doesn't match '{tag}' tag. Check for incorrect date entered.";
                FactErrorLevel = FactError.ERROR;
                //                FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                return;
            }
            if (tag == "Census" || tag == "LostCousins" || tag == "Lost Cousins")
            {
                TimeSpan ts = FactDate.EndDate - FactDate.StartDate;
                if (ts.Days > 3650)
                {
                    //                    FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                    FactErrorLevel = FactError.ERROR;
                    FactErrorMessage = "Date covers more than one census.";
                    return;
                }
            }
            if (tag == "Census")
            {
                if (!CensusDate.IsCensusYear(yearAdjusted, Country, false))
                {
                    //                    FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                    FactErrorMessage = $"Census fact error date '{FactDate}' isn't a supported census date. Check for incorrect date entered or try Tolerate slightly inaccurate census date option.";
                    FactErrorLevel = FactError.ERROR;
                    return;
                }
                if (GeneralSettings.Default.TolerateInaccurateCensusDate && yearAdjusted.IsKnown && !CensusDate.IsCensusYear(yearAdjusted, Country, true))
                {
                    //                    FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                    FactErrorMessage = $"Warning : Census fact error date '{FactDate}' overlaps census date but is vague. Check for incorrect date entered.";
                    FactErrorLevel = FactError.WARNINGALLOW;
                }
                if (!FactDate.Equals(yearAdjusted))
                    FactDate = yearAdjusted;
            }
            if (tag == "Lost Cousins" || tag == "LostCousins")
            {
                if (GeneralSettings.Default.TolerateInaccurateCensusDate && yearAdjusted.IsKnown && !CensusDate.IsLostCousinsCensusYear(yearAdjusted, true))
                {
                    //                    FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                    FactErrorMessage = $"Lost Cousins fact error date '{FactDate}' overlaps Lost Cousins census year but is vague. Check for incorrect date entered.";
                    FactErrorLevel = Fact.FactError.WARNINGALLOW;
                }
                if (!CensusDate.IsLostCousinsCensusYear(yearAdjusted, false))
                {
                    //                    FactErrorNumber = (int)FamilyTree.Dataerror.CENSUS_COVERAGE;
                    FactErrorMessage = $"Lost Cousins fact error date '{FactDate}' isn't a supported Lost Cousins census year. Check for incorrect date entered or try Tolerate slightly inaccurate census date option.";
                    FactErrorLevel = FactError.ERROR;
                }
                if (!FactDate.Equals(yearAdjusted))
                    FactDate = yearAdjusted;
            }
        }

        void SetCommentAndLocation(string factType, string factComment, string factPlace, string addrTagText, string latitude, string longitude)
        {
            if (factComment.Length == 0 && factPlace.Length > 0)
            {
                if (factPlace.EndsWith("/", StringComparison.Ordinal))
                {
                    Comment = factPlace.Substring(0, factPlace.Length - 1);
                    Place = string.Empty;
                }
                else
                {
                    int slash = factPlace.IndexOf("/", StringComparison.Ordinal);
                    if (slash >= 0)
                    {
                        Comment = factPlace.Substring(0, slash).Trim();
                        // If slash occurs at end of string, location is empty.
                        Place = (slash == factPlace.Length - 1) ? string.Empty : factPlace.Substring(slash + 1).Trim();
                    }
                    else if (COMMENT_FACTS.Contains(factType))
                    {
                        // we have a comment rather than a location
                        Comment = factPlace;
                        Place = string.Empty;
                    }
                    else
                    {
                        Comment = string.Empty;
                        Place = factPlace;
                    }
                }
            }
            else
            {
                Comment = factComment;
                Place = factPlace;
                if (factType == NAME)
                    Comment = Comment.Replace("/", "");
            }
            Comment = EnhancedTextInfo.ToTitleCase(Comment).Trim();
            if (string.IsNullOrEmpty(latitude))
                latitude = "0.0";
            if (string.IsNullOrEmpty(longitude))
                longitude = "0.0";
            FactLocation.Geocode geocode = 
                (latitude.Equals("0.0") && longitude.Equals("0.0")) ? FactLocation.Geocode.NOT_SEARCHED : FactLocation.Geocode.GEDCOM_USER;
            if (addrTagText.Length > 0)
            {    //we have an address decide to add it to place or not
                if (string.IsNullOrEmpty(Place) || addrTagText.Contains(Place))
                    Place = addrTagText;
                else if (GeneralSettings.Default.ReverseLocations)
                    Place = $"{Place}, {addrTagText}";
                else
                    Place = $"{addrTagText}, {Place}";
            }
            if (GeneralSettings.Default.ReverseLocations)
                Location = FactLocation.GetLocation(ReverseLocation(Place), latitude, longitude, geocode, true, true);
            else
                Location = FactLocation.GetLocation(Place, latitude, longitude, geocode, true, true);
            Location.FTAnalyzerCreated = false;
        }

        bool SetCertificatePresent()
        {
            return Sources.Any(fs =>
            {
                return (FactType.Equals(BIRTH) && fs.IsBirthCert()) ||
                    (FactType.Equals(DEATH) && fs.IsDeathCert()) ||
                    (FactType.Equals(MARRIAGE) && fs.IsMarriageCert()) ||
                    (FactType.Equals(CENSUS) && fs.IsCensusCert());
            });
        }

        string UnknownFactHash => FactType == UNKNOWN ? Tag : string.Empty;

        string FamilyFactHash => Family is null ? string.Empty : Family.FamilyID;
        
        public string PossiblyEqualHash => FactType + UnknownFactHash + FamilyFactHash +  FactDate + IsMarriageFact;

        public string EqualHash => FactType + UnknownFactHash + FamilyFactHash + FactDate + Location + Comment + IsMarriageFact;

        public bool IsValidCensus(FactDate factDate) => FactDate.IsKnown && IsCensusFact && FactDate.FactYearMatches(factDate) && FactDate.IsNotBEForeOrAFTer && FactErrorLevel == FactError.GOOD;

        public bool IsValidCensus(CensusDate censusDate) => FactDate.IsKnown && IsCensusFact && FactDate.CensusYearMatches(censusDate) && FactDate.IsNotBEForeOrAFTer && FactErrorLevel == FactError.GOOD;

        public bool IsValidLostCousins(CensusDate censusDate) => 
            FactDate.IsKnown && (FactType == LOSTCOUSINS || FactType == LC_FTA) &&
            FactDate.CensusYearMatches(censusDate) && FactDate.IsNotBEForeOrAFTer && FactErrorLevel == FactError.GOOD;

        public bool IsOverseasUKCensus(string country) =>
            country.Equals(Countries.OVERSEAS_UK) || (!Countries.IsUnitedKingdom(country) && CensusReference != null && CensusReference.IsUKCensus);

        public override string ToString() => 
            FactTypeDescription + ": " + FactDate + (Location.ToString().Length > 0 ? " at " + Location : string.Empty) + (Comment.Length > 0 ? "  (" + Comment + ")" : string.Empty);
    }
}
