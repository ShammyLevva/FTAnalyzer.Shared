using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using FTAnalyzer.Windows;
using HtmlAgilityPack;

namespace FTAnalyzer.Exports
{
    public static class ExportToLostCousins
    {
        static List<CensusIndividual> ToProcess { get; set; }
        static List<LostCousin> Website { get; set; }
        static List<LostCousin> SessionList { get; set; }
        static List<Uri> WebLinks { get; set; }

        public static async Task<int> ProcessListAsync(List<CensusIndividual> individuals, IProgress<string> outputText)
        {
            if (individuals is null) return 0;
            int recordsAdded = 0;
            try
            {
                ToProcess = individuals;
                int recordsFailed = 0;
                int recordsPresent = 0;
                int sessionDuplicates = 0;
                int count = 0;
                Dictionary<string, string> dummy;
                Website ??= await LoadWebsiteAncestorsAsync(outputText);
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
                    else if (ind.CensusReference != null && ind.CensusReference.IsValidLostCousinsReference())
                    {
                        dummy = new();
                        string reference = Program.LCClient.GetCensusSpecificFields(dummy, ind);
                        LostCousin lc = new($"{ind.SurnameAtDate(ind.CensusDate)}, {ind.Forenames}", ind.BirthDate.BestYear, reference, ind.CensusDate.BestYear, ind.CensusCountry, true);
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
                                if (await Program.LCClient.AddIndividualToWebsiteAsync(ind, outputText))
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
                await Analytics.TrackActionAsync(Analytics.LostCousinsAction, Analytics.ReadLostCousins, $"{DateTime.Now.ToUniversalTime():yyyy-MM-dd HH:mm}: {manualfacts} manual & {ftanalyzerfacts} -> {ftanalyzerfacts + recordsAdded} FTAnalyzer entries");
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
            Individual person = FamilyTree.Instance.GetIndividual(ind.IndividualID); // get the individual not the census indvidual
            if(person != null && !person.HasLostCousinsFactAtDate(ind.CensusDate))
                person.AddFact(f);
        }

        public static async Task<SortableBindingList<IDisplayLostCousin>> VerifyAncestorsAsync(IProgress<string> outputText)
        {
            SortableBindingList<IDisplayLostCousin> result = new();
            Website ??= await LoadWebsiteAncestorsAsync(outputText);
            WebLinks = new List<Uri>();
            foreach(LostCousin lostCousin in Website)
            {
                result.Add(lostCousin);
                if (!WebLinks.Contains(lostCousin.WebLink))
                    WebLinks.Add(lostCousin.WebLink);
            }
            return result;
        }

        static async Task<List<LostCousin>> LoadWebsiteAncestorsAsync(IProgress<string> outputText)
        {
            List<LostCousin> websiteList = new();
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new();
                string webData = await Program.LCClient.GetAncestors();
                doc.LoadHtml(webData);
                HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table[@class='data_table']/tr");
                if (rows != null)
                {
                    foreach (HtmlNode node in rows)
                    {
                        HtmlNodeCollection columns = node.SelectNodes("td");
                        if (columns != null && columns.Count == 8 && columns[0].InnerText != "Name") // ignore header row
                        {
                            string name = columns[0].InnerText.ClearWhiteSpace();
                            bool ftanalyzer = false;
                            string weblink = string.Empty;
                            if (columns[0].ChildNodes.Count == 5)
                            {
                                HtmlAttribute notesNode = columns[0].ChildNodes[3].Attributes["title"];
                                ftanalyzer = notesNode != null && notesNode.Value.Contains("Added_By_FTAnalyzer");
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
                return null;
            }
            return websiteList;
        }
    }
}
