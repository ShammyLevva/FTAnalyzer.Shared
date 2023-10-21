using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FTAnalyzer.Exports
{
    public static class ExportToLostCousins
    {
        static List<CensusIndividual> ToProcess { get; set; }
        static NetworkCredential Credentials { get; set; }
        static CookieCollection CookieJar { get; set; }
        static List<LostCousin> Website { get; set; }
        static List<LostCousin> SessionList { get; set; }
        static List<Uri> WebLinks { get; set; }
        static string _previousRef;

        public static int ProcessList(List<CensusIndividual> individuals, IProgress<string> outputText)
        {
            if (individuals is null) return 0;
            int recordsAdded = 0;
            try
            {
                ToProcess = individuals;
                _previousRef = string.Empty;
                int recordsFailed = 0;
                int recordsPresent = 0;
                int sessionDuplicates = 0;
                int count = 0;
                Website ??= LoadWebsiteAncestors(outputText);
                SessionList ??= new List<LostCousin>();
                bool alias = GeneralSettings.Default.ShowAliasInName;
                GeneralSettings.Default.ShowAliasInName = false; // turn off adding alias in name when exporting
                foreach (CensusIndividual ind in ToProcess)
                {
                    if (ind.LCAge.Equals("Unknown"))
                    {
                        outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Cannot determine age at census {ind.CensusString}.\n");
                        recordsFailed++;
                    }
                    else if (ind.LCSurnameAtDate(ind.CensusDate).Length == 0 || ind.LCForename.Length == 0)
                    {
                        outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Cannot process person with unknown forename or surname {ind.CensusString}.\n");
                        recordsFailed++;
                    }
                    else if (ind.CensusReference is not null && ind.CensusReference.IsValidLostCousinsReference())
                    {
                        LostCousin lc = new($"{ind.SurnameAtDate(ind.CensusDate)}, {ind.Forenames}", ind.BirthDate.BestYear, GetCensusSpecificFields(ind), ind.CensusDate.BestYear, ind.CensusCountry, true);
                        if (Website.Contains(lc))
                        {
                            outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Already Present {ind.CensusString}, {ind.CensusReference}.\n");
                            if (!DatabaseHelper.LostCousinsExists(ind))
                                DatabaseHelper.StoreLostCousinsFact(ind, outputText);
                            AddLostCousinsFact(ind);
                            recordsPresent++;
                        }
                        else
                        {
                            if (SessionList.Contains(lc))
                            {
                                outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Already submitted this session {ind.CensusString}, {ind.CensusReference}. Possible duplicate Individual\n");
                                sessionDuplicates++;
                            }
                            else
                            {
                                if (AddIndividualToWebsite(ind, outputText))
                                {
                                    outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - {ind.CensusString}, {ind.CensusReference} added.\n");
                                    recordsAdded++;
                                    SessionList.Add(lc);
                                    if (!DatabaseHelper.LostCousinsExists(ind))
                                        DatabaseHelper.StoreLostCousinsFact(ind, outputText);
                                    AddLostCousinsFact(ind);
                                }
                                else
                                {
                                    outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Failed to add {ind.CensusString}, {ind.CensusReference}.\n");
                                    recordsFailed++;
                                }
                            }
                        }
                    }
                    else
                    {
                        outputText.Report($"Record {++count} of {ToProcess.Count}: {ind.CensusDate} - Failed to add {ind.CensusString}, {ind.CensusReference}. Census Reference problem.\n");
                        recordsFailed++;
                    }
                }
                GeneralSettings.Default.ShowAliasInName = alias;
                outputText.Report($"\nFinished writing Entries to Lost Cousins website. {recordsAdded} successfully added, {recordsPresent} already present, {sessionDuplicates} possible duplicates and {recordsFailed} failed.\nView Lost Cousins Report tab to see current status.\n");
                outputText.Report("\nPlease note you MUST check the entries by clicking the arrow next to the census reference on the list on my ancestors page.\n");
                outputText.Report("This only needs done once per household and will link to the census on Find My Past.\n");
                outputText.Report("If you have any errors you can correct them on your my ancestors page. The most common will be Age and different spelling of names.\n");
                outputText.Report("Occasionally you may have got a census reference wrong in which case the page either wont exist or will show the wrong family.");
                outputText.Report("\n\nNote if you fail to check your entries you will fail to match with your Lost Cousins.");
                int ftanalyzerfacts = Website.FindAll(lc => lc.FTAnalyzerFact).Count;
                int manualfacts = Website.FindAll(lc => !lc.FTAnalyzerFact).Count;
                Task.Run(() => Analytics.TrackActionAsync(Analytics.LostCousinsAction, Analytics.ReadLostCousins, $"{DateTime.Now.ToUniversalTime():yyyy-MM-dd HH:mm}: {manualfacts} manual & {ftanalyzerfacts} -> {ftanalyzerfacts + recordsAdded} FTAnalyzer entries"));
            }
            catch(Exception e)
            {
                UIHelpers.ShowMessage($"Problem uploading to Lost Cousins error was : {e.Message}");
            }
            return recordsAdded;
        }

        static void AddLostCousinsFact(CensusIndividual ind)
        {
            FactLocation location = FactLocation.GetLocation(ind.CensusCountry);
            Fact f = new(ind.CensusRef, Fact.LC_FTA, ind.CensusDate, location, string.Empty, true, true);
            Individual? person = FamilyTree.Instance.GetIndividual(ind.IndividualID); // get the individual not the census indvidual
            if(person is not null && !person.HasLostCousinsFactAtDate(ind.CensusDate))
                person.AddFact(f);
        }

        public static bool CheckLostCousinsLogin(string email, string password)
        {
            HttpWebResponse resp = null;
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;
                if (password.Length > 15)
                    password = password[..15];
                string formParams = $"stage=submit&email={HttpUtility.UrlEncode(email)}&password={password}{Suffix()}";
                HttpWebRequest req = WebRequest.Create(new Uri("https://www.lostcousins.com/pages/login/")) as HttpWebRequest;
                req.Referer = "https://www.lostcousins.com/pages/login/";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                Credentials = new NetworkCredential(email, password);
                req.Credentials = Credentials;
                req.CookieContainer = new CookieContainer();
                req.AllowAutoRedirect = false;
                byte[] bytes = Encoding.ASCII.GetBytes(formParams);
                req.ContentLength = bytes.Length;
                req.Timeout = 10000;
                using (Stream os = req.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                resp = req.GetResponse() as HttpWebResponse;
                CookieJar = resp.Cookies;
                return CookieJar.Count == 2 && (CookieJar[0].Name == "lostcousins_user_login" || CookieJar[1].Name == "lostcousins_user_login");
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

        public static SortableBindingList<IDisplayLostCousin> VerifyAncestors(IProgress<string> outputText)
        {
            SortableBindingList<IDisplayLostCousin> result = new();
            Website ??= LoadWebsiteAncestors(outputText);
            WebLinks = new List<Uri>();
            foreach(LostCousin lostCousin in Website)
            {
                result.Add(lostCousin as IDisplayLostCousin);
                if (!WebLinks.Contains(lostCousin.WebLink))
                    WebLinks.Add(lostCousin.WebLink);
            }
            return result;
        }

        //static bool OnPreRequest(HttpWebRequest request)
        //{
        //    request.AllowAutoRedirect = true;
        //    return true;
        //}

        public static void EmptyCookieJar() => CookieJar = null;

        static List<LostCousin> LoadWebsiteAncestors(IProgress<string> outputText)
        {
            List<LostCousin> websiteList = new();
            try
            {
                using CookieAwareWebClient wc = new(CookieJar);
                HtmlAgilityPack.HtmlDocument doc = new();
                string webData = wc.DownloadString("https://www.lostcousins.com/pages/members/ancestors/");
                doc.LoadHtml(webData);
                HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table[@class='data_table']/tr");
                if (rows is not null)
                {
                    foreach (HtmlNode node in rows)
                    {
                        HtmlNodeCollection columns = node.SelectNodes("td");
                        if (columns is not null && columns.Count == 8 && columns[0].InnerText != "Name") // ignore header row
                        {
                            string name = columns[0].InnerText.ClearWhiteSpace();
                            bool ftanalyzer = false;
                            string weblink = string.Empty;
                            if (columns[0].ChildNodes.Count == 5)
                            {
                                HtmlAttribute notesNode = columns[0].ChildNodes[3].Attributes["title"];
                                ftanalyzer = notesNode is not null && notesNode.Value.Contains("Added_By_FTAnalyzer");
                            }
                            string birthYear = columns[2].InnerText.ClearWhiteSpace();
                            if (columns[4].ChildNodes.Count > 4)
                            {
                                string weblinkText = columns[4].ChildNodes[3].OuterHtml;
                                if (weblinkText.Length > 10)
                                {
                                    int pos = weblinkText.IndexOf('"', 10);
                                    if (pos > -1)
                                        weblink = weblinkText[9..pos].Trim();
                                }
                            }
                            string reference = columns[4].InnerText.ClearWhiteSpace();
                            string census = columns[5].InnerText.ClearWhiteSpace();
                            LostCousin lc = new(name, birthYear, reference, census, weblink, ftanalyzer);
                            websiteList.Add(lc);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                outputText.Report($"\nProblem accessing Lost Cousins Website to read current ancestor list. Error message is: {e.Message}\n");
                return new List<LostCousin>();
            }
            return websiteList;
        }

        static bool AddIndividualToWebsite(CensusIndividual ind, IProgress<string> outputText)
        {
            if (ind is null) return false;
            HttpWebResponse resp = null;
            try
            {
                string formParams = BuildParameterString(ind);
                HttpWebRequest req = WebRequest.Create(new Uri("https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml")) as HttpWebRequest;
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
                if (e.Message.Contains("UNIQUE constraint failed:")) // already written so silently ignore adding to database.
                    return true;
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
            StringBuilder output = new("stage=submit");
            string newRef = GetCensusSpecificFields(ind);
            if (newRef == _previousRef)
                output.Append("&similar=1");
            else
                output.Append("&similar=");
            _previousRef = newRef;
            output.Append(newRef);
            output.Append($"&surname={ind.LCSurnameAtDate(ind.CensusDate)}");
            output.Append($"&forename={ind.LCForename}");
            output.Append($"&other_names={ind.LCOtherNames}");
            output.Append($"&age={ind.LCAge}");
            output.Append($"&relation_type={GetLCDescendantStatus(ind)}");
            if (!ind.IsMale && ind.LCSurname != ind.LCSurnameAtDate(ind.CensusDate))
                output.Append($"&maiden_name={ind.LCSurname}");
            else
                output.Append("&maiden_name=");
            output.Append($"&corrected_surname={ind.LCSurnameAtDate(ind.CensusDate)}&corrected_forename={ind.LCForename}&corrected_other_names={ind.LCOtherNames}");
            if (ind.BirthDate.IsExact)
                output.Append($"&corrected_birth_day={ind.BirthDate.StartDate.Day}&corrected_birth_month={ind.BirthDate.StartDate.Month}&corrected_birth_year={ind.BirthDate.StartDate.Year}");
            else
                output.Append($"&corrected_birth_day=&corrected_birth_month=&corrected_birth_year=");
            output.Append("&baptism_day=&baptism_month=&baptism_year=");
            output.Append($"&piece_number=&notes=Added_By_FTAnalyzer-{FamilyTree.Instance.Version}{Suffix()}");
            return output.ToString();
        }

        static string Suffix()
        {
            Random random = new();
            int x = random.Next(1,99);
            int y = random.Next(1, 9);
            return $"&x={x}&y={y}";
        }

        static string GetCensusSpecificFields(CensusIndividual ind)
        {
            CensusReference censusRef = ind.CensusReference;
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=1841&ref1={censusRef.Piece}&ref2={censusRef.Book}&ref3={censusRef.Folio}&ref4={censusRef.Page}&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=RG11&ref1={censusRef.Piece}&ref2={censusRef.Folio}&ref3={censusRef.Page}&ref4=&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.SCOTCENSUS1881) && ind.CensusCountry == Countries.SCOTLAND)
                return $"&census_code=SCT1&ref1={censusRef.RD}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.CANADACENSUS1881) && ind.CensusCountry == Countries.CANADA)
                return $"&census_code=CAN1&ref1={censusRef.ED}&ref2={censusRef.SD}&ref3=&ref4={censusRef.Page}&ref5={censusRef.Family}";
            //if (ind.CensusDate.Overlaps(CensusDate.IRELANDCENSUS1911) && ind.CensusCountry == Countries.IRELAND)
            //    return $"&census_code=0IRL&ref1=&ref2=&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(ind.CensusCountry))
                return $"&census_code=0ENG&ref1={censusRef.Piece}&ref2={censusRef.Schedule}&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.USCENSUS1880) && ind.CensusCountry == Countries.UNITED_STATES)
                return $"&census_code=USA1&ref1={censusRef.Roll}&ref2={censusRef.Page}&ref3=&ref4=&ref5=";
            if (ind.CensusDate.Overlaps(CensusDate.USCENSUS1940) && ind.CensusCountry == Countries.UNITED_STATES)
                return $"&census_code=USA4&ref1={censusRef.Roll}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            return string.Empty;
        }

        static string GetLCDescendantStatus(CensusIndividual ind)
        {
            return ind.RelationType switch
            {
                Individual.DIRECT => $"Direct+ancestor&descent={ind.Ahnentafel}",
                Individual.BLOOD or Individual.DESCENDANT => "Blood+relative&descent=",
                Individual.MARRIAGE or Individual.MARRIEDTODB => "Marriage&descent=",
                Individual.UNKNOWN or Individual.UNSET or Individual.LINKED => "Unknown&descent=",
                _ => "Unknown&descent=",
            };
        }
    }

    class CookieAwareWebClient : WebClient
    {
        readonly CookieContainer _cookieJar = new();

        internal CookieAwareWebClient(CookieCollection cookies)
        {
            _cookieJar.Add(cookies);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest webRequest)
                webRequest.CookieContainer = _cookieJar;
            return request;
        }
    }
}
