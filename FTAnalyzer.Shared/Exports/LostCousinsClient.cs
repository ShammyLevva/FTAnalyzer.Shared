using FTAnalyzer.Utilities;
using System.Net;

namespace FTAnalyzer.Exports
{
    public class LostCousinsClient
    {
        public HttpClient Client { get; private set; }
        public CookieContainer Cookies { get; private set; }

        public bool LoggedIn { get; private set; }

        public LostCousinsClient()
        {
            SetupHttpClient();
        }

        void SetupHttpClient()
        {
            Cookies = new();
            HttpClientHandler handler = new()
            {
                CookieContainer = Cookies
            };
            Client = new(handler);
        }
        public async Task<bool> LostCousinsLoginAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;
                if (password.Length > 15)
                    password = password[..15];
                Random random = new();
                Uri uri = new(@"https://www.lostcousins.com/pages/login/");
                Dictionary<string, string> parameters = new()
                {
                    { "stage", "submit" },
                    { "email", email },
                    { "password", password },
                    { "x", random.Next(1,99).ToString() },
                    { "y", random.Next(1,9).ToString() }
                };
                LoggedIn = false;
                HttpRequestMessage req = new(HttpMethod.Post, uri)
                {
                    Content = new FormUrlEncodedContent(parameters)
                };
                req.Content.Headers.Clear();
                req.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                HttpResponseMessage response = await Client.SendAsync(req);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    IEnumerable<Cookie> cookies = Cookies.GetCookies(uri).Cast<Cookie>();
                    LoggedIn = cookies.Count() == 2 && (cookies.Any(x => x.Name == "lostcousins_user_login" || x.Name == "lostcousins_user_login"));
                }
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Problem accessing Lost Cousins Website. Check you are connected to internet. Error message is: {e.Message}");
                return false;
            }
            return LoggedIn;
        }

        public async Task<string> GetAncestors()
        {
            if (LoggedIn)
            {
                Uri uri = new("https://www.lostcousins.com/pages/members/ancestors/");
                HttpResponseMessage response = await Client.GetAsync(uri);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            return string.Empty;
        }

        public async Task<bool> AddIndividualToWebsiteAsync(CensusIndividual ind, IProgress<string> outputText)
        {
            if (ind is null || !LoggedIn) return false;

            try
            {
                Dictionary<string, string> formParams = BuildParameterString(ind);
                Uri uri = new("https://www.lostcousins.com/pages/members/ancestors/add_ancestor.mhtml");
                HttpRequestMessage req = new(HttpMethod.Post, uri)
                {
                    Content = new FormUrlEncodedContent(formParams)
                };
                req.Content.Headers.Clear();
                req.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                HttpResponseMessage response = await Client.SendAsync(req);
                return response?.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("UNIQUE constraint failed:")) // already written so silently ignore adding to database.
                    return true;
                outputText.Report($"Problem accessing Lost Cousins Website to send record below. Error message is: {e.Message}\n");
                return false;
            }
        }

        string _previousRef;

        Dictionary<string, string> BuildParameterString(CensusIndividual ind)
        {

            Dictionary<string, string> output = new()
            {
                { "stage", "submit" }
            };
            string newRef = GetCensusSpecificFields(output, ind);
            if (newRef == _previousRef)
                output.Add("similar", "1");
            else
                output.Add("similar", string.Empty);
            _previousRef = newRef;

            output.Add("surname", ind.LCSurnameAtDate(ind.CensusDate));
            output.Add("forename", ind.LCForename);
            output.Add("other_names", ind.LCOtherNames);
            output.Add("age", ind.LCAge);
            AddLCDescendantStatus(output, ind);
            if (!ind.IsMale && ind.LCSurname != ind.LCSurnameAtDate(ind.CensusDate))
                output.Add("maiden_name", ind.LCSurname);
            else
                output.Add("maiden_name", string.Empty);
            output.Add("corrected_surname", ind.LCSurnameAtDate(ind.CensusDate));
            output.Add("corrected_forename", ind.LCForename);
            output.Add("corrected_other_names", ind.LCOtherNames);
            if (ind.BirthDate.IsExact)
            {
                output.Add("corrected_birth_day", ind.BirthDate.StartDate.Day.ToString());
                output.Add("corrected_birth_month", ind.BirthDate.StartDate.Month.ToString());
                output.Add("corrected_birth_year", ind.BirthDate.StartDate.Year.ToString());
            }
            else
            {
                output.Add("corrected_birth_day", string.Empty);
                output.Add("corrected_birth_month", string.Empty);
                output.Add("corrected_birth_year", string.Empty);
            }
            output.Add("baptism_day", string.Empty);
            output.Add("baptism_month", string.Empty);
            output.Add("baptism_year", string.Empty);
            output.Add("piece_number", string.Empty);
            output.Add("notes", $"Added_By_FTAnalyzer-{FamilyTree.Instance.Version}");
            Random random = new();
            output.Add("x", random.Next(1, 99).ToString());
            output.Add("y", random.Next(1, 9).ToString());
            return output;
        }

        public string GetCensusSpecificFields(Dictionary<string, string> output, CensusIndividual ind)
        {
            CensusReference? censusRef = ind.CensusReference;
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1841) && Countries.IsEnglandWales(ind.CensusCountry))
            {
                output.Add("census_code", "1841");
                output.Add("ref1", censusRef.Piece);
                output.Add("ref2", censusRef.Book);
                output.Add("ref3", censusRef.Folio);
                output.Add("ref4", censusRef.Page);
                output.Add("ref5", string.Empty);
                return $"&census_code=1841&ref1={censusRef.Piece}&ref2={censusRef.Book}&ref3={censusRef.Folio}&ref4={censusRef.Page}&ref5=";
            }
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1881) && Countries.IsEnglandWales(ind.CensusCountry))
            {
                output.Add("census_code", "RG11");
                output.Add("ref1", censusRef.Piece);
                output.Add("ref2", censusRef.Folio);
                output.Add("ref3", censusRef.Page);
                output.Add("ref4", string.Empty);
                output.Add("ref5", string.Empty);
                return $"&census_code=RG11&ref1={censusRef.Piece}&ref2={censusRef.Folio}&ref3={censusRef.Page}&ref4=&ref5=";
            }
            if (ind.CensusDate.Overlaps(CensusDate.SCOTCENSUS1881) && ind.CensusCountry == Countries.SCOTLAND)
            {
                output.Add("census_code", "SCT1");
                output.Add("ref1", censusRef.RD);
                output.Add("ref2", censusRef.ED);
                output.Add("ref3", censusRef.Page);
                output.Add("ref4", string.Empty);
                output.Add("ref5", string.Empty);
                return $"&census_code=SCT1&ref1={censusRef.RD}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            }
            if (ind.CensusDate.Overlaps(CensusDate.CANADACENSUS1881) && ind.CensusCountry == Countries.CANADA)
            {
                output.Add("census_code", "CAN1");
                output.Add("ref1", censusRef.ED);
                output.Add("ref2", censusRef.SD);
                output.Add("ref3", string.Empty);
                output.Add("ref4", censusRef.Page);
                output.Add("ref5", censusRef.Family);
                return $"&census_code=CAN1&ref1={censusRef.ED}&ref2={censusRef.SD}&ref3=&ref4={censusRef.Page}&ref5={censusRef.Family}";
            }
            //if (ind.CensusDate.Overlaps(CensusDate.IRELANDCENSUS1911) && ind.CensusCountry == Countries.IRELAND)
            //{
            //    output.Add("census_code", "0IRL");
            //    output.Add("ref1", string.Empty);
            //    output.Add("ref2", string.Empty);
            //    output.Add("ref3", string.Empty);
            //    output.Add("ref4", string.Empty);
            //    output.Add("ref5", string.Empty);
            //    return $"&census_code=0IRL&ref1=&ref2=&ref3=&ref4=&ref5=";
            //}
            if (ind.CensusDate.Overlaps(CensusDate.EWCENSUS1911) && Countries.IsEnglandWales(ind.CensusCountry))
            {
                output.Add("census_code", "0ENG");
                output.Add("ref1", censusRef.Piece);
                output.Add("ref2", censusRef.Schedule);
                output.Add("ref3", string.Empty);
                output.Add("ref4", string.Empty);
                output.Add("ref5", string.Empty);
                return $"&census_code=0ENG&ref1={censusRef.Piece}&ref2={censusRef.Schedule}&ref3=&ref4=&ref5=";
            }
            if (ind.CensusDate.Overlaps(CensusDate.USCENSUS1880) && ind.CensusCountry == Countries.UNITED_STATES)
            {
                output.Add("census_code", "USA1");
                output.Add("ref1", censusRef.Roll);
                output.Add("ref2", censusRef.Page);
                output.Add("ref3", string.Empty);
                output.Add("ref4", string.Empty);
                output.Add("ref5", string.Empty);
                return $"&census_code=USA1&ref1={censusRef.Roll}&ref2={censusRef.Page}&ref3=&ref4=&ref5=";
            }
            if (ind.CensusDate.Overlaps(CensusDate.USCENSUS1940) && ind.CensusCountry == Countries.UNITED_STATES)
            {
                output.Add("census_code", "USA4");
                output.Add("ref1", censusRef.Roll);
                output.Add("ref2", censusRef.ED);
                output.Add("ref3", censusRef.Page);
                output.Add("ref4", string.Empty);
                output.Add("ref5", string.Empty);
                return $"&census_code=USA4&ref1={censusRef.Roll}&ref2={censusRef.ED}&ref3={censusRef.Page}&ref4=&ref5=";
            }
            return string.Empty;
        }

        public void EmptyCookieJar()
        {
            SetupHttpClient();
        }

        static void AddLCDescendantStatus(Dictionary<string, string> output, CensusIndividual ind)
        {
            switch (ind.RelationType)
            {
                case Individual.DIRECT:
                    output.Add("relation_type", "Direct+ancestor");
                    output.Add("descent", ind.Ahnentafel.ToString());
                    break;
                case Individual.BLOOD:
                case Individual.DESCENDANT:
                    output.Add("relation_type", "Blood+relative");
                    output.Add("descent", string.Empty);
                    break;
                case Individual.MARRIAGE:
                case Individual.MARRIEDTODB:
                    output.Add("relation_type", "Marriage");
                    output.Add("descent", string.Empty);
                    break;
                case Individual.UNKNOWN:
                case Individual.UNSET:
                case Individual.LINKED:
                    output.Add("relation_type", "Unknown");
                    output.Add("descent", string.Empty);
                    break;
                default:
                    output.Add("relation_type", "Unknown");
                    output.Add("descent", string.Empty);
                    break;
            };
        }
    }
}