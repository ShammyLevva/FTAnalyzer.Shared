using System.Text;
using System.Data;
using System.IO;
#if __PC__
using System;
using System.Windows.Forms;
#elif __MACOS__
using AppKit;
using static FTAnalyzer.UIHelpers;
#endif

namespace FTAnalyzer.Utilities
{
    public static class ExportToExcel
    {
#if __PC__
        public static void Export(DataTable dt)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string initialDir = (string)Application.UserAppDataRegistry.GetValue("Excel Export Individual Path");
                saveFileDialog.InitialDirectory = initialDir ?? Environment.SpecialFolder.MyDocuments.ToString();
                saveFileDialog.Filter = "Comma Separated Value (*.csv)|*.csv";
                saveFileDialog.FilterIndex = 1;
                DialogResult dr = saveFileDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string path = Path.GetDirectoryName(saveFileDialog.FileName);
                    Application.UserAppDataRegistry.SetValue("Excel Export Individual Path", path);
                    WriteFile(dt, saveFileDialog.FileName);
                    UIHelpers.ShowMessage($"File written to {saveFileDialog.FileName}", "FTAnalyzer");
                }
            }
            catch (Exception ex)
            {
                UIHelpers.ShowMessage(ex.Message, "FTAnalyzer");
            }
        }
#elif __MACOS__
        public static void Export(DataTable dt, string exportType)
        {
            var dlg = NSSavePanel.SavePanel;
            dlg.Title = "Export data to Excel";
            dlg.AllowedFileTypes = new string[] { "csv" };
            dlg.Message = "Select location to export file to";
            dlg.NameFieldStringValue = exportType;
            var result = dlg.RunModal();
            if (result == 1) // ok
            {
                WriteFile(dt, dlg.Url.Path);
                ShowMessage($"File written to {dlg.Url.Path}", "FTAnalyzer");
            }
        }
#endif
        static void WriteFile(DataTable table, string filename)
        {
            string q = "\"";
            StreamWriter output = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8);
            //am getting my grid's column headers
            int columnscount = table.Columns.Count;

            for (int j = 0; j < columnscount; j++)
            {   //Get column headers  and make it as bold in excel columns
                var column = table.Rows[0][j];
                if (column.ToString() != "System.Drawing.Bitmap")
                {
                    output.Write(q + table.Columns[j].ColumnName + q);
                    if (j < columnscount - 1)
                        output.Write(",");
                }
            }
            output.WriteLine();
            foreach (DataRow row in table.Rows)
            {
                //write in new row
                for (int col = 0; col < columnscount; col++)
                {
                    var cell = row[col];
                    if (cell.ToString() != "System.Drawing.Bitmap")
                    {
                        output.Write(q + row[col].ToString().Replace("\"", "") + q);
                        if (col < columnscount - 1)
                            output.Write(",");
                    }
                }
                output.WriteLine();
            }
            output.Close();
        }
    }
}
