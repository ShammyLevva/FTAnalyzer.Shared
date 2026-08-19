using FTAnalyzer.Utilities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FTAnalyzer.Exports
{
    /// <summary>
    /// Helper tool for the old, retired National Archives of Ireland census page URL (e.g.
    /// "http://www.census.nationalarchives.ie/pages/1911/Longford/Longford_No__1_Urban/
    /// Main_Street/651470/") that some GEDCOM citations still carry from years ago. The trailing
    /// number is an "image_group" ID from the archive's search index - a completely different,
    /// unrelated numbering scheme from the 9-digit reel number (e.g. "nai002908773")
    /// CensusReference/RegexPatterns already recognise as a valid Lost Cousins reference (see
    /// IRELAND_CENSUS_1911_PATTERN in RegexPatterns.cs). There's no formula connecting the two -
    /// confirmed live against the archive's own site: the same household's image_group 651470
    /// resolves to reel nai002908773, while neighbouring reel numbers for the SAME household's
    /// other form types run 002908688-002908705, i.e. reel numbers are contiguous per household
    /// but the starting offset has no derivable relationship to image_group. No public finding
    /// aid exists to bridge the two offline (checked), so this can only ever be resolved via a
    /// live lookup - deliberately kept OUT of CensusReference's offline parsing, as a standalone
    /// opt-in helper tool instead.
    ///
    /// Both FTAnalyzer.Windows and FTAnalyzer.Web drive this exact same code (see
    /// IrishCensusHelperForm on desktop, IrishCensusHelper.razor on web) - the only thing that
    /// differs per platform is how progress/results get displayed.
    /// </summary>
    public static partial class IrishCensusHelperScanner
    {
        // Captures the trailing "image_group" number from the old per-record page URL - the only
        // part the archive's own lookup API actually needs (see IrishCensusHelperClient).
        [GeneratedRegex(@"census\.nationalarchives\.ie/pages/1911/\S*?/(\d+)/?", RegexOptions.IgnoreCase)]
        private static partial Regex OldStyleUrlPattern();

        /// <summary>
        /// Scans every census fact on every individual for a citation containing the old-style
        /// URL. Deliberately reads CensusReference.Reference rather than anything in
        /// CensusReference's own recognised-pattern parsing - when a citation isn't recognised,
        /// BuildReference falls through to returning the original raw text (prefixed "Unknown
        /// Census Ref: "), which is what lets this find the URL without CensusReference or
        /// RegexPatterns needing to know anything about it.
        /// </summary>
        public static List<IrishCensusHelperRecord> FindOldStyleReferences(IEnumerable<Individual> individuals)
        {
            List<IrishCensusHelperRecord> found = [];
            foreach (Individual ind in individuals)
            {
                foreach (Fact f in ind.AllFacts)
                {
                    if (!f.IsCensusFact || f.CensusReference is null)
                        continue;
                    Match m = OldStyleUrlPattern().Match(f.CensusReference.Reference);
                    if (m.Success)
                        found.Add(new IrishCensusHelperRecord(ind, f, m.Value, m.Groups[1].Value));
                }
            }
            return found;
        }
    }

    /// <summary>
    /// One matched old-style citation, plus (once IrishCensusHelperProcessor.ResolveAllAsync has
    /// run) the resolved reel URL. ColumnDetail attributes let this feed both platforms' CSV/Excel
    /// exporters directly with no per-platform DTO.
    /// </summary>
    public sealed class IrishCensusHelperRecord(Individual individual, Fact fact, string oldUrl, string imageGroup)
    {
        public Individual Individual { get; } = individual;
        public Fact Fact { get; } = fact;

        [ColumnDetail("Name", 200)]
        public string Name => $"{Individual.Forename} {Individual.Surname}";

        [ColumnDetail("Old Citation URL", 400)]
        public string OldUrl { get; } = oldUrl;

        public string ImageGroup { get; } = imageGroup;

        [ColumnDetail("Resolved URL", 400)]
        public string? ResolvedUrl { get; set; }

        [ColumnDetail("Status", 120)]
        public string Status { get; set; } = "Not yet checked";
    }

    /// <summary>
    /// Calls the National Archives of Ireland's own public census-search API directly - confirmed
    /// live (this session) to be a plain, unauthenticated JSON endpoint the archive's own
    /// JS-rendered search page calls behind the scenes, at a separate subdomain from the main
    /// site: GET https://api-census.nationalarchives.ie/census/query?image_group={n}&amp;limit=50.
    /// No headless browser or HTML scraping needed. Owns its own HttpClient, matching
    /// LostCousinsClient/Program.Client/Program.LCClient's convention - there's no DI container
    /// anywhere in this codebase and this doesn't introduce one.
    /// </summary>
    public class IrishCensusHelperClient
    {
        const string ApiBase = "https://api-census.nationalarchives.ie/census/query";

        // A courteous fixed floor delay between lookups, matching GeoapifyGeocodingService's
        // documented self-throttle - no published rate limit exists for this API, but nothing
        // about scanning someone's whole tree needs to hammer a third party's server to do it.
        static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(500);

        static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        readonly HttpClient client = new();
        DateTime lastCallUtc = DateTime.MinValue;

        /// <summary>
        /// Looks up one image_group and returns the direct PDF URL for the household's Form A,
        /// side 1 image - the same "nai&lt;9 digits&gt;" reel URL CensusReference/RegexPatterns
        /// already recognise as a valid Lost Cousins citation (IRELAND_CENSUS_1911_PATTERN) - or
        /// null if the archive has no record for that image_group, or the lookup failed.
        /// </summary>
        public async Task<string?> ResolveAsync(string imageGroup, CancellationToken ct)
        {
            try
            {
                TimeSpan sinceLastCall = DateTime.UtcNow - lastCallUtc;
                if (sinceLastCall < MinInterval)
                    await Task.Delay(MinInterval - sinceLastCall, ct);
                lastCallUtc = DateTime.UtcNow;

                using HttpResponseMessage response = await client.GetAsync(
                    $"{ApiBase}?image_group={Uri.EscapeDataString(imageGroup)}&limit=50", ct);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync(ct);
                return ExtractFormAUrl(json);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return null; // network hiccup, JSON shape change, etc. - caller sees "Not found" and moves on
            }
        }

        /// <summary>
        /// Pulls the household's Form A, side 1 image out of a raw
        /// api-census.nationalarchives.ie/census/query response body and turns it into the direct
        /// PDF URL. Split out from ResolveAsync so the parsing itself is testable against a
        /// captured response body without a live network call - see IrishCensusHelperTest.
        /// </summary>
        public static string? ExtractFormAUrl(string json)
        {
            CensusQueryResponse? data = JsonSerializer.Deserialize<CensusQueryResponse>(json, JsonOptions);
            CensusImage? formA = data?.Results?
                .Where(r => r.Images is not null)
                .SelectMany(r => r.Images!)
                .FirstOrDefault(img => string.Equals(img.Form, "Form A", StringComparison.OrdinalIgnoreCase)
                    && img.Side == "1");
            return formA?.Id is null ? null : $"https://nai.prod.derilinx.com/census/image/{formA.Id}.pdf";
        }

        // Response shapes for https://api-census.nationalarchives.ie/census/query - only the
        // fields this lookup actually needs are modelled; everything else the API returns
        // (surname, age, religion, etc.) is ignored.
        sealed class CensusQueryResponse
        {
            public List<CensusQueryResult>? Results { get; set; }
        }

        sealed class CensusQueryResult
        {
            public List<CensusImage>? Images { get; set; }
        }

        sealed class CensusImage
        {
            public string? Form { get; set; }
            public string? Side { get; set; }
            public string? Id { get; set; }
        }
    }

    /// <summary>
    /// The per-record resolve loop both platforms drive identically - desktop wraps this in
    /// Task.Run from a WinForms Click handler, web calls it directly from a Blazor page. Adds
    /// cancellation support, which the older ExportToLostCousins.ProcessListAsync precedent has
    /// no equivalent of - this follows FamilyTree.GenerateDuplicatesList/GeocodeLocations's
    /// cancellable-loop idiom instead, since a user manually stepping through a whole tree of
    /// live lookups needs a way to bail out partway through.
    /// </summary>
    public static class IrishCensusHelperProcessor
    {
        public static async Task ResolveAllAsync(List<IrishCensusHelperRecord> records,
            IrishCensusHelperClient client, IProgress<string> outputText, CancellationToken token)
        {
            int count = 0;
            foreach (IrishCensusHelperRecord record in records)
            {
                token.ThrowIfCancellationRequested();
                record.ResolvedUrl = await client.ResolveAsync(record.ImageGroup, token);
                record.Status = record.ResolvedUrl is not null ? "Resolved" : "Not found";
                outputText.Report($"Record {++count} of {records.Count}: {record.Name} - {record.Status}");
            }
        }
    }
}
