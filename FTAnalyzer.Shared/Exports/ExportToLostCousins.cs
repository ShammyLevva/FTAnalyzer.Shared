using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace FTAnalyzer.Exports
{
    public static class ExportToLostCousins
    {
        static List<CensusIndividual> ToProcess { get; set; }
        static NetworkCredential Credentials { get; set; }
        static CookieCollection CookieJar { get; set; }
        
        public static int ProcessList(List<CensusIndividual> individuals, IProgress<string> outputText)
        {
            ToProcess = individuals;
            int recordsAdded = 0;
            int count = 0;
            foreach (CensusIndividual ind in ToProcess)
            {
                if (AddIndividualToWebsite(ind, outputText))
                {
                    outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - {ind.ToString()}, {ind.CensusReference} added.\n");
                    recordsAdded++;
#if __PC__
                    DatabaseHelper.Instance.StoreLostCousinsFact(ind);
#endif
                }
                else
                    outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Failed to add {ind.ToString()}, {ind.CensusReference}.\n");
            }
            outputText.Report("\n\nFinished writing Entries to Lost Cousins website");
            return recordsAdded;
        }

        public static bool CheckLostCousinsLogin(string email, string password)
        {
            HttpWebResponse resp = null;
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;
                string formParams = $"stage=submit&email={HttpUtility.UrlEncode(email)}&password={password}{Suffix()}";
                HttpWebRequest req = WebRequest.Create("https://www.lostcousins.com/pages/login/") as HttpWebRequest;
                req.Referer = "https://www.lostcousins.com/pages/login/";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                Credentials = new NetworkCredential(email, password);
                req.Credentials = Credentials;
                req.CookieContainer = new CookieContainer();
                req.AllowAutoRedirect = false;
                byte[] bytes = Encoding.ASCII.GetBytes(formParams);
                req.ContentLength = bytes.Length;
                using (Stream os = req.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                resp = req.GetResponse() as HttpWebResponse;
                CookieJar = resp.Cookies;
                return CookieJar.Count == 2 && (CookieJar[0].Name=="lostcousins_user_login" || CookieJar[1].Name == "lostcousins_user_login");
            }
            catch(Exception e)
            {
                UIHelpers.ShowMessage($"Problem accessing Lost Cousins Website. Check you are connected to internet. Error message is: {e.Message}");
                return false;
            }
            finally
            {
                resp?.Close();
            }
        }

        static bool AddIndividualToWebsite(CensusIndividual ind, IProgress<string> outputText)
        {
            HttpWebResponse resp = null;
            try
            {
                string formParams = BuildParameterString(ind);
                HttpWebRequest req = WebRequest.Create("https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml") as HttpWebRequest;
                req.Referer = "https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                req.Credentials = Credentials;
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.Add(CookieJar);
                byte[] bytes = Encoding.ASCII.GetBytes(formParams);
                req.ContentLength = bytes.Length;
                req.Timeout = 10000;
                using (Stream os = req.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                resp = req.GetResponse() as HttpWebResponse;
                return resp.ResponseUri.Query.Length > 0;
            }
            catch (Exception e)
            {
                outputText.Report($"Problem accessing Lost Cousins Website to send record below. Error message is: {e.Message}\n");
                return false;
            }
            finally
            {
                resp?.Close();
            }
        }

        static string BuildParameterString(CensusIndividual ind)
        {
            StringBuilder output = new StringBuilder("stage=submit&similar=");
            output.Append(GetCensusSpecificFields(ind));
            output.Append($"&surname={ind.SurnameAtDate(ind.CensusDate)}");
            output.Append($"&forename={ind.Forename}");
            output.Append($"&other_names={ind.OtherNames}");
            output.Append($"&age={ind.LCAge}");
            output.Append($"&relation_type={GetLCDescendantStatus(ind)}");
            if (!ind.IsMale && ind.Surname != ind.SurnameAtDate(ind.CensusDate))
                output.Append($"&maiden_name={ind.Surname}");
            else
                output.Append("&maiden_name=");
            output.Append($"&corrected_surname=&corrected_forename=&corrected_other_names=");
            if (ind.BirthDate.IsExact)
                output.Append($"&corrected_birth_day={ind.BirthDate.StartDate.Day}&corrected_birth_month={ind.BirthDate.StartDate.Month}&corrected_birth_year={ind.BirthDate.StartDate.Year}");
            else
                output.Append($"&corrected_birth_day=&corrected_birth_month=&corrected_birth_year=");
            output.Append("&baptism_day=&baptism_month=&baptism_year=");
            output.Append($"&piece_number=&notes=Added_By_FTAnalyzer{Suffix()}"); 
            return output.ToString();
        }

        static string Suffix()
        {
            Random random = new Random();
            int x = random.Next(1,99);
            int y = random.Next(1, 9);
            return $"&x={x}&y={y}";
        }

        static string GetCensusSpecificFields(CensusIndividual ind)
        {
            CensusReference censusRef = ind.CensusReference;
            if (ind.CensusDate.Equals(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=1841&ref1={censusRef.Piece}&ref2={censusRef.Book}&ref3={censusRef.Folio}&ref4={censusRef.Page}&ref5=";
            if (ind.CensusDate.Equals(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=RG11&ref1={censusRef.Piece}&ref2={censusRef.Folio}&ref3={censusRef.Page}&ref4=&ref5=";
            if (ind.CensusDate.Equals(CensusDate.SCOTCENSUS1881) && ind.CensusCountry == Countries.SCOTLAND)
                return $"&census_code=SCT1&ref1={censusRef.RD}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            if (ind.CensusDate.Equals(CensusDate.CANADACENSUS1881) && ind.CensusCountry == Countries.CANADA)
                return $"&census_code=CAN1&ref1={censusRef.ED}&ref2={censusRef.SD}&ref3=&ref4={censusRef.Page}&ref5={censusRef.Family}";
            //if (ind.CensusDate.Equals(CensusDate.IRELANDCENSUS1911) && ind.CensusCountry == Countries.IRELAND)
            //    return $"&census_code=0IRL&ref1=&ref2=&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Equals(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=0ENG&ref1={censusRef.Piece}&ref2={censusRef.Schedule}&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Equals(CensusDate.USCENSUS1880) && ind.CensusCountry == Countries.UNITED_STATES)
                return $"&census_code=USA1&ref1={censusRef.Roll}&ref2={censusRef.Page}&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Equals(CensusDate.USCENSUS1940) && ind.CensusCountry == Countries.UNITED_STATES)
                return $"&census_code=USA4&ref1={censusRef.Roll}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            return string.Empty;
        }

        static string GetLCDescendantStatus(CensusIndividual ind)
        {
            switch(ind.RelationType)
            {
                case Individual.DIRECT:
                    return $"Direct+ancestor&descent={ind.Ahnentafel}";
                case Individual.BLOOD: 
                case Individual.DESCENDANT:
                    return "Blood+relative&descent=";
                case Individual.MARRIAGE:
                case Individual.MARRIEDTODB:
                    return "Marriage&descent=";
                case Individual.UNKNOWN:
                case Individual.UNSET:
                default:
                    return "Unknown&descent=";
            }
        }
    }
}
