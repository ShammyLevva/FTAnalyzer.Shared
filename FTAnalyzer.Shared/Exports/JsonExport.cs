using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FTAnalyzer.Exports
{
    public class JsonExport
    {
        static readonly FamilyTree ft = FamilyTree.Instance;

        public JsonExport(string filename)
        {
            Filename = filename;
            Individuals = new List<IJsonIndividual>(ft.AllIndividuals);
            Families = new List<IJsonFamily>(ft.AllFamilies);
            //var facts = ft.AllExportFacts;
        }

        #region Serialisable Properties
        public static string VersionNumber => "1.0.0";
        public string Filename { get; }
        public static string ExportDate => DateTime.Now.ToString("dd MMM yyyy HH:mm");
        public List<IJsonIndividual> Individuals { get; }
        public List<IJsonFamily> Families { get; }

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
