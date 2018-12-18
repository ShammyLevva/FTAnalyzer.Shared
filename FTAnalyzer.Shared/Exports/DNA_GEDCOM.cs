using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if __PC__
using System.Windows.Forms;
#elif __MACOS__
using AppKit;
#endif

namespace FTAnalyzer.Exports
{
    public static class DNA_GEDCOM
    {
        static readonly FactDate PrivacyDate = new FactDate(DateTime.Now.AddYears(-100).ToString("dd MMM yyyy"));
        static readonly FactDate Today = new FactDate(DateTime.Now.ToString("dd MMM yyyy"));
        static FamilyTree ft = FamilyTree.Instance;
        static bool _includeSiblings = false;
#if __MACOS__
        static AppDelegate App => (AppDelegate)NSApplication.SharedApplication.Delegate;
#endif 

#if __PC__
        public static void Export()
        {
            DialogResult result = MessageBox.Show("Do you want to include siblings of direct ancestors in the export?", "FTAnalyzer", MessageBoxButtons.YesNoCancel);
            if (result != DialogResult.Cancel)
            {
                _includeSiblings = result == DialogResult.Yes;
                try
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "FTAnalyzer");
                }
            }
        }
#elif __MACOS__
        public static void Export()
        {

        }
#endif
        static void WriteFile(string filename)
        {
            List<Family> families = new List<Family>();
            StreamWriter output = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8);
            WriteHeader(filename, output);
            foreach (Individual ind in ft.DirectLineIndividuals)
            {
                WriteIndividual(ind, output);
                foreach (ParentalRelationship asChild in ind.FamiliesAsChild)
                {
                    if (asChild.IsNaturalFather || asChild.IsNaturalMother)
                    {
                        output.WriteLine($"1 FAMC @{asChild.Family.FamilyID}@");
                        if(!families.Contains(asChild.Family))
                            families.Add(asChild.Family);
                    }
                }
                foreach (Family asParent in ind.FamiliesAsSpouse)
                {
                    output.WriteLine($"1 FAMS @{asParent.FamilyID}@");
                    if (!families.Contains(asParent))
                        families.Add(asParent);
                }
            }
            if (_includeSiblings)
                WriteSiblings(families, output);
            WriteFamilies(families, output);
            WriteFooter(output);
            output.Close();
#if __PC__
            MessageBox.Show("Minimalist GEDCOM file written for use with DNA Matching. Upload today.");
#elif __MACOS__
            UIHelpers.ShowMessage("Minimalist GEDCOM file written for use with DNA Matching. Upload today.");
#endif
        }

        static void WriteSiblings(List<Family> families, StreamWriter output)
        {
            foreach (Family fam in families)
            {
                foreach (Individual child in fam.Children)
                {
                    if (child.RelationType != Individual.DIRECT && child.RelationType != Individual.DESCENDANT) // only write out siblings not directs at this point
                        WriteIndividual(child, output);
                }
            }
        }

        static void WriteFamilies(List<Family> families, StreamWriter output)
        {
            foreach(Family fam in families)
            {
                bool isPrivate = fam.FamilyDate.IsAfter(PrivacyDate) &&
                                ((fam.Husband != null && fam.Husband.IsAlive(Today)) ||
                                 (fam.Wife != null && fam.Wife.IsAlive(Today))); // if marriage is after privacy date and either party is alive then make marriage private
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

        static void WriteHeader(string filename, StreamWriter output)
        {
#if __PC__
            var version = MainForm.VERSION;
#elif __MACOS__
            var version = App.Version;
#endif
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

        static void WriteIndividual(Individual ind, StreamWriter output)
        {
            bool isPrivate = ind.BirthDate.IsAfter(PrivacyDate) && ind.IsAlive(Today);
            output.WriteLine($"0 @{ind.IndividualID}@ INDI");
            if (isPrivate)
            {
                output.WriteLine("1 NAME Private /Person/");
                output.WriteLine("2 GIVN Private");
                output.WriteLine("2 SURN Person");
            }
            else
            {
                output.WriteLine($"1 NAME {ind.Forename} /{ind.Surname}/");
                output.WriteLine($"2 GIVN {ind.Forename}");
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
        }

        static void WriteFooter(StreamWriter output) => output.WriteLine("0 TRLR");
    }
}
