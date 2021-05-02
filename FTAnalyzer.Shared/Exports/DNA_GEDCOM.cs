using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if __PC__
using FTAnalyzer.Utilities;
using System.Windows.Forms;
#elif __MACOS__
using AppKit;
#elif __IOS__
using UIKit;
#endif

namespace FTAnalyzer.Exports
{
    public static class DNA_GEDCOM
    {
        static readonly FactDate PrivacyDate = new FactDate(FactDate.NOW.AddYears(-100).ToString("dd MMM yyyy", FactDate.CULTURE));
        static readonly FamilyTree ft = FamilyTree.Instance;
        static bool _includeSiblings;
        static bool _includeDescendants;
        static bool _privatise;
        static int _privateID;
        static List<Individual> processed;
        static StreamWriter output;
#if __MACOS__
        static AppDelegate App => (AppDelegate)NSApplication.SharedApplication.Delegate;
#elif __IOS__
        static AppDelegate App => (AppDelegate)UIApplication.SharedApplication.Delegate;
#endif

        public static void Export()
        {
            int privatise = UIHelpers.ShowYesNo("Do you want living people replaced with 'Private Person' and their details hidden");
            if (privatise != UIHelpers.Cancel)
            {
                int siblingsResult = UIHelpers.ShowYesNo("Do you want to include SIBLINGS of direct ancestors in the export?");
                if (siblingsResult != UIHelpers.Cancel)
                {
                    int descendantsResult = UIHelpers.No;
                    if (siblingsResult == UIHelpers.Yes) // only ask about descendants if including siblings
                        descendantsResult = UIHelpers.ShowYesNo("Do you want to include DESCENDANTS of siblings in the export?");
                    if (descendantsResult != UIHelpers.Cancel)
                    {
                        _privatise = privatise == UIHelpers.Yes;
                        _includeSiblings = siblingsResult == UIHelpers.Yes;
                        _includeDescendants = descendantsResult == UIHelpers.Yes;
                        try
                        {
                            ExportGedcomFile();
                        }
                        catch (Exception ex)
                        {
                            UIHelpers.ShowMessage(ex.Message, "FTAnalyzer");
                        }
                        finally
                        {
                            output?.Close();
                        }
                    }
                }
            }
        }
#if __PC__
        public static void ExportGedcomFile()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string initialDir = (string)Application.UserAppDataRegistry.GetValue("Export DNA GEDCOM Path");
                saveFileDialog.InitialDirectory = initialDir ?? Environment.SpecialFolder.MyDocuments.ToString();
                saveFileDialog.Filter = "Comma Separated Value (*.ged)|*.ged";
                saveFileDialog.FilterIndex = 1;
                DialogResult dr = saveFileDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string path = Path.GetDirectoryName(saveFileDialog.FileName);
                    Application.UserAppDataRegistry.SetValue("Export DNA GEDCOM Path", path);
                    WriteFile(saveFileDialog.FileName);
                }
            }
        }
#elif __MACOS__
        public static void ExportGedcomFile()
        {
            var dlg = NSSavePanel.SavePanel;
            dlg.Title = "Export GEDCOM File of skeleton tree";
            dlg.AllowedFileTypes = new string[] { "ged" };
            dlg.Message = "Select location to export GEDCOM file to";
            dlg.NameFieldStringValue = "Minimalist DNA GEDCOM";
            var response = dlg.RunModal();
            if (response == 1)
                WriteFile(dlg.Url.Path);
         }
#elif __IOS__
        public static void ExportGedcomFile()
        {
        // feature not available on iOS
        }
#endif
        static void WriteFile(string filename)
        {
            List<Family> families = new List<Family>();
            List<Individual> spouses = new List<Individual>();
            _privateID = 1;
            using (output = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
            {
                WriteHeader(filename);
                processed = new List<Individual>();
                foreach (Individual ind in ft.DirectLineIndividuals)
                {
                    WriteIndividual(ind);
                    foreach (ParentalRelationship asChild in ind.FamiliesAsChild)
                    {
                        if (asChild.IsNaturalFather || asChild.IsNaturalMother)
                        {
                            output.WriteLine($"1 FAMC @{asChild.Family.FamilyID}@");
                            if (!families.Contains(asChild.Family))
                                families.Add(asChild.Family);
                        }
                    }
                    foreach (Family asSpouse in ind.FamiliesAsSpouse)
                    {
                        output.WriteLine($"1 FAMS @{asSpouse.FamilyID}@");
                        var spouse = asSpouse.Spouse(ind);
                        if (spouse?.RelationType != Individual.DIRECT && spouse?.RelationType != Individual.DESCENDANT)
                            spouses.Add(spouse); // we have a spouse that isn't a direct so is a step relation add to list to write
                        if (!families.Contains(asSpouse))
                            families.Add(asSpouse);
                    }
                }
                WriteSpouses(spouses);
                if (_includeSiblings)
                    WriteSiblings(families);
                WriteFamilies(families);
                WriteFooter();
            }
            UIHelpers.ShowMessage("Minimalist GEDCOM file written for use with DNA Matching. Upload today.");
        }

        static void WriteSiblings(List<Family> families)
        {
            List<Individual> descendants = new List<Individual>();
            foreach (Family fam in families)
            {
                foreach (Individual child in fam.Children)
                { 
                    if (child.RelationType != Individual.DIRECT && child.RelationType != Individual.DESCENDANT) // only write out siblings not directs at this point
                    {
                        if(_includeDescendants)
                            descendants.Add(child); // add to list of all descendants to write out
                        else
                            WriteIndividual(child);
                    }
                }
            }
            if (_includeDescendants)
                WriteDescendants(descendants);
        }

        static void WriteSpouses(List<Individual> spouses)
        {
            foreach (Individual spouse in spouses)
                WriteIndividual(spouse);
        }

        static void WriteDescendants(List<Individual> descendants)
        {
            Queue<Individual> queue = new Queue<Individual>();
            foreach (Individual i in descendants)
                if(i.IsBloodDirect)
                    queue.Enqueue(i);
            Individual ind;
            List<Family> descendantFamilies = new List<Family>();
            while(queue.Count > 0)
            {
                ind = queue.Dequeue();
                if(ind.IsBloodDirect)
                    WriteIndividual(ind);
                foreach(Family fam in ind.FamiliesAsSpouse)
                {
                    if(fam.Husband != null && fam.Husband.IsBloodDirect)
                        WriteIndividual(fam.Husband);
                    if (fam.Wife != null && fam.Wife.IsBloodDirect)
                        WriteIndividual(fam.Wife);
                    foreach (Individual child in fam.Children)
                        queue.Enqueue(child);
                    descendantFamilies.Add(fam);
                }
            }
            WriteFamilies(descendantFamilies);
        }

        static void WriteFamilies(List<Family> families)
        {
            foreach(Family fam in families)
            {
                bool isPrivate = _privatise && fam.FamilyDate.IsAfter(PrivacyDate) &&
                                ((fam.Husband != null && fam.Husband.IsAlive(FactDate.TODAY)) ||
                                 (fam.Wife != null && fam.Wife.IsAlive(FactDate.TODAY))); // if marriage is after privacy date and either party is alive then make marriage private
                output.WriteLine($"0 @{fam.FamilyID}@ FAM");
                if(fam.Husband != null)
                    output.WriteLine($"1 HUSB @{fam.HusbandID}@");
                if (fam.Wife != null)
                    output.WriteLine($"1 WIFE @{fam.WifeID}@");
                foreach(Individual child in fam.Children)
                {
                    if(_includeSiblings || child.RelationType == Individual.DIRECT || child.RelationType == Individual.DESCENDANT) // skip siblings if not including
                        output.WriteLine($"1 CHIL @{child.IndividualID}@");
                }
                if (!isPrivate)
                {
                    output.WriteLine($"1 MARR");
                    output.WriteLine($"2 DATE {fam.MarriageDate}");
                    output.WriteLine($"2 PLAC {fam.MarriageLocation}");
                }
            }
        }

        static void WriteHeader(string filename)
        {
            var version = FamilyTree.Instance.Version;
            output.WriteLine($"0 HEAD");
            output.WriteLine($"1 SOUR Family Tree Analyzer");
            output.WriteLine($"2 VERS {version}");
            output.WriteLine($"2 NAME Family Tree Analyzer");
            output.WriteLine($"2 CORP FTAnalyzer.com");
            output.WriteLine($"1 FILE {filename}");
            output.WriteLine($"1 GEDC");
            output.WriteLine($"2 VERS 5.5.1");
            output.WriteLine($"2 FORM LINEAGE - LINKED");
            output.WriteLine($"1 CHAR UTF-8");
            output.WriteLine($"1 _ROOT @{ft.RootPerson.IndividualID}@");
        }

        static void WriteIndividual(Individual ind)
        {
            if (ind is null || processed.Contains(ind))
                return; // don't write out individual if already processed
            bool isPrivate = _privatise && ind.BirthDate.IsAfter(PrivacyDate) && ind.IsAlive(FactDate.TODAY);
            output.WriteLine($"0 @{ind.IndividualID}@ INDI");
            if (isPrivate)
            {
                output.WriteLine($"1 NAME Private {_privateID} /Person/");
                output.WriteLine($"2 GIVN Private {_privateID}");
                output.WriteLine("2 SURN Person");
                _privateID++;
            }
            else
            {
                output.WriteLine($"1 NAME {ind.Forenames} /{ind.Surname}/");
                output.WriteLine($"2 GIVN {ind.Forenames}");
                output.WriteLine($"2 SURN {ind.Surname}");
            }
            output.WriteLine($"1 SEX {(ind.IsMale ? 'M' : 'F')}");
            if (!isPrivate)
            {
                if (ind.BirthDate.IsKnown)
                {
                    output.WriteLine($"1 BIRT");
                    output.WriteLine($"2 DATE {ind.BirthDate}");
                    output.WriteLine($"2 PLAC {ind.BirthLocation}");
                }
                if(ind.DeathDate.IsKnown)
                {
                    output.WriteLine($"1 DEAT");
                    output.WriteLine($"2 DATE {ind.DeathDate}");
                    output.WriteLine($"2 PLAC {ind.DeathLocation}");
                }
            }
            processed.Add(ind);
        }

        static void WriteFooter() => output.WriteLine("0 TRLR");
    }
}
