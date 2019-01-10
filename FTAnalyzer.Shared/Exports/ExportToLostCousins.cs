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
        static NetworkCredential Credential { get; set; }
        static CookieCollection CookieJar { get; set; }
        
        public static void ProcessList(List<CensusIndividual> individuals)
        {
            ToProcess = individuals;
            foreach (CensusIndividual ind in ToProcess)
                if (!AddIndividualToWebsite(ind)) return;
        }

        public static bool CheckLostCousinsLogin(string email, string password)
        {
            HttpWebResponse resp = null;
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;
                string formParams = $"stage=submit&email={HttpUtility.UrlEncode(email)}&password={password}&x=51&y=5";
                HttpWebRequest req = WebRequest.Create("https://www.lostcousins.com/pages/login/") as HttpWebRequest;
                req.Referer = "https://www.lostcousins.com/pages/login/";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                Credential = new NetworkCredential(email, password);
                req.Credentials = Credential;
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

        static bool AddIndividualToWebsite(CensusIndividual ind)
        {
            HttpWebResponse resp = null;
            try
            {
                string formParams = BuildParameterString(ind);
                HttpWebRequest req = WebRequest.Create("https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml") as HttpWebRequest;
                req.Referer = "https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                req.Credentials = Credential;
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.Add(CookieJar);
                byte[] bytes = Encoding.ASCII.GetBytes(formParams);
                req.ContentLength = bytes.Length;
                using (Stream os = req.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                resp = req.GetResponse() as HttpWebResponse;
                return resp.ResponseUri.AbsoluteUri != req.Referer;
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Problem accessing Lost Cousins Website. Check you are connected to internet. Error message is: {e.Message}");
                return false;
            }
            finally
            {
                resp?.Close();
            }
        }

        static string BuildParameterString(CensusIndividual ind)
        {
            StringBuilder output = new StringBuilder("stage=change_census&similar=");
            output.Append(GetCensusSpecificFields(ind));
            return output.ToString();
        }

        static string GetCensusSpecificFields(CensusIndividual ind)
        {
            if (ind.CensusDate.Equals(CensusDate.EWCENSUS1881))
                return $"&census_code=RG11&ref1={ind.CensusReference.Piece}&ref2={ind.CensusReference.Folio}&ref3={ind.CensusReference.Page}";
            //if (ind.CensusDate.Equals(CensusDate.SCOTCENSUS1881))
            //    return "&census_code=SCT1";
            //if (ind.CensusDate.Equals(CensusDate.CANADACENSUS1881))
            //    return "&census_code=CAN1";
            //if (ind.CensusDate.Equals(CensusDate.USCENSUS1880))
            //    return "&census_code=USA1";
            //if (ind.CensusDate.Equals(CensusDate.EWCENSUS1841))
            //    return "&census_code=1841";
            //if (ind.CensusDate.Equals(CensusDate.IRELANDCENSUS1911))
            //    return "&census_code=0IRL";
            //if (ind.CensusDate.Equals(CensusDate.EWCENSUS1911))
            //    return "&census_code=0ENG";
            //if (ind.CensusDate.Equals(CensusDate.USCENSUS1940))
            //    return "&census_code=USA4";
            //if (ind.CensusDate.Equals(CensusDate.EWCENSUS1881))
            //    return "NEWF";
            return string.Empty;
        }
    }
}
