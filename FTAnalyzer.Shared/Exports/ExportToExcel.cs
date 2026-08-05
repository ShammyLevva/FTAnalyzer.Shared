using FTAnalyzer.Shared.Utilities;
using Microsoft.Win32;
using System.Data;
using System.Text;

namespace FTAnalyzer.Utilities
{
    public static class ExportToExcel
    {
#if __PC__
        public static void Export(DataTable dt)
        {
            try
            {
                if (dt.Rows.Count > 0)
                {
                    string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    using SaveFileDialog saveFileDialog = new();
                    string initialDir = RegistrySettings.GetStringRegistryValue("Excel Export Individual Path", myDocuments);
                    saveFileDialog.InitialDirectory = initialDir ?? myDocuments;
                    saveFileDialog.Filter = "Comma Separated Value (*.csv)|*.csv";
                    saveFileDialog.FilterIndex = 1;
                    DialogResult dr = saveFileDialog.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string path = Path.GetDirectoryName(saveFileDialog.FileName) ?? string.Empty;
                        RegistrySettings.SetRegistryValue("Excel Export Individual Path", path, RegistryValueKind.String);
                        WriteFile(dt, saveFileDialog.FileName);
                        UIHelpers.ShowMessage($"File written to {saveFileDialog.FileName}", "FTAnalyzer");
                    }
                }
                else
                    UIHelpers.ShowMessage("No records to export from that list.");
            }
            catch (Exception ex)
            {
                UIHelpers.ShowMessage(ex.Message, "FTAnalyzer");
            }
        }
#endif
        static void WriteFile(DataTable table, string filename)
        {
            string q = "\"";
            using StreamWriter output = new(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8);
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
                        output.Write(q + (row[col]?.ToString() ?? string.Empty).Replace("\"", "", StringComparison.Ordinal).Replace("\n", " :: ", StringComparison.Ordinal) + q);
                        if (col < columnscount - 1)
                            output.Write(",");
                    }
                }
                output.WriteLine();
            }
        }
    }
}
