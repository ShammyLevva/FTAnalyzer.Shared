using Newtonsoft.Json;

namespace FTAnalyzer.Exports
{
    public class JsonExport(string filename)
    {
        static readonly FamilyTree ft = FamilyTree.Instance;

        #region Serialisable Properties
        public static string VersionNumber => "1.0.0";
        public string Filename { get; } = filename;
        public static string ExportDate => DateTime.Now.ToString("dd MMM yyyy HH:mm");
        public List<IJsonIndividual> Individuals { get; } = [.. ft.AllIndividuals];
        public List<IJsonFamily> Families { get; } = [.. ft.AllFamilies];

        #endregion

        public void WriteJsonData(StreamWriter output)
        {
            if (output is null) return;
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            output.Write(JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings));
        }
    }
}
