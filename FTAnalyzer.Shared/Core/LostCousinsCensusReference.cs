using FTAnalyzer.Utilities;
using System.Text.RegularExpressions;

namespace FTAnalyzer
{
    public class LostCousinsCensusReference : CensusReference
    {
        public bool IsValid { get; }
        static readonly Regex EDregex = new Regex(@"\d{1,3}[A-Z]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        LostCousinsCensusReference() : base()
        {
            IsValid = CheckValidity();
        }

        bool CheckValidity()
        { 
            if (Status != ReferenceStatus.GOOD)
                return false;
            //use Peter's code to check all the entries are valid
            if (CensusYear.Equals(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("HO107"))
                    Piece = Piece.Substring(5);
                Piece = Piece.TrimStart('0');
                Book = Book.TrimStart('0');
                Folio = Folio.TrimStart('0').ToUpper().TrimEnd('A');
                if (!Piece.IsNumeric() || !Folio.IsNumeric() || !Page.IsNumeric()) return false;
            }
            else if (CensusYear.Equals(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG78"))
                    return false;
                if (Piece.StartsWith("RG11"))
                    Piece = Piece.Substring(4);
                Piece = Piece.TrimStart('0');
                Folio = Folio.TrimStart('0').ToUpper().TrimEnd('A');
                if (!Piece.IsNumeric() || !Folio.IsNumeric() || !Page.IsNumeric()) return false;
            }
            else if (CensusYear.Equals(CensusDate.SCOTCENSUS1881) && Country == Countries.SCOTLAND)
            {
                Parish = Parish.Replace('/', '-').TrimStart('0').TrimEnd('-');
                if (Parish.Length > 0)
                {
                    if (ScottishParish.IsParishID(Parish))
                        RD = Parish;
                    else
                    {
                        ScottishParish sp = ScottishParish.FindParishFromID(Parish);
                        if (sp.RegistrationDistrict != "UNK")
                            RD = sp.RegistrationDistrict;
                        else
                        {
                            RD = ScottishParish.FindParishFromName(Parish);
                            if (RD == "Unknown")
                            {
                                Status = ReferenceStatus.INCOMPLETE;
                                return false;
                            }
                        }
                    }
                }
                else
                    return false;
                ED = ED.TrimStart('0');
                Page = Page.TrimStart('0');
                if (!Page.IsNumeric()) return false;
                Match match = EDregex.Match(ED); //also check d{1,3}[A-Z]? format
                if (!match.Success) return false; // check last to only do regex calc if everything else is ok
            }
            else if (CensusYear.Equals(CensusDate.CANADACENSUS1881) && Country == Countries.CANADA)
            {
                if (Roll.ToUpper().StartsWith("C_"))
                    Roll = Roll.Substring(2);


            }
            else if (CensusYear.Equals(CensusDate.IRELANDCENSUS1911) && Country == Countries.IRELAND)
            {
            }
            else if (CensusYear.Equals(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(Country))
            {
                if (Piece.StartsWith("RG14"))
                    Piece = Piece.Substring(4);
                if (Piece.StartsWith("PN"))
                    Piece = Piece.Substring(2);
                Piece = Piece.TrimStart('0');
                Schedule = Schedule.TrimStart('0');
                if (Schedule.Length == 0 && Page.Length > 0)
                    Schedule = "9999";
                if (!Piece.IsNumeric()) return false;
            }
            else if (CensusYear.Equals(CensusDate.USCENSUS1880) && Country == Countries.UNITED_STATES)
            {
                if (Roll.ToUpper().StartsWith("T9"))
                    Roll = Roll.Substring(2);
                Roll = Roll.TrimStart('-').TrimStart('_').TrimStart('0');
                Page = NumericToAlpha(Page.TrimStart('0'));
                if (!Roll.IsNumeric()) return false;
             }
            else if (CensusYear.Equals(CensusDate.USCENSUS1940) && Country == Countries.UNITED_STATES)
            {
                Roll = Roll.ToUpper();
                if (Roll.StartsWith("T627_")) Roll = Roll.Substring(5);
                else if (Roll.StartsWith("T0627_")) Roll = Roll.Substring(6);
                else if (Roll.StartsWith("M_T627_")) Roll = Roll.Substring(7);
                else if (Roll.StartsWith("M_T0627_")) Roll = Roll.Substring(8);
                Roll = Roll.TrimStart('0');
                Page = NumericToAlpha(Page.TrimStart('0'));
                if (!Roll.IsNumeric()) return false;
            }
            return true;
        }

        string NumericToAlpha(string page)
        {
            if (page.Length > 3)
            {
                string prefix = page.Substring(0, page.Length - 2);
                if (Page.EndsWith(".1")) return prefix + "A";
                if (Page.EndsWith(".2")) return prefix + "B";
                if (Page.EndsWith(".3")) return prefix + "C";
                if (Page.EndsWith(".4")) return prefix + "D";
                if (Page.EndsWith(".5")) return prefix + "E";
                if (Page.EndsWith(".6")) return prefix + "F";
                if (Page.EndsWith(".7")) return prefix + "G";
                if (Page.EndsWith(".8")) return prefix + "H";
                if (Page.EndsWith(".9")) return prefix + "I";
            }
            return page;
        }
    }
}
