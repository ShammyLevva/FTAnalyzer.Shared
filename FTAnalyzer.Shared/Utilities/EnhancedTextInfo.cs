using FTAnalyzer.Properties;
using System.Globalization;
using System.Text;

namespace FTAnalyzer.Utilities
{
    public static class EnhancedTextInfo
    {
        static readonly TextInfo txtInfo = new CultureInfo("en-GB").TextInfo;

        public static string ToTitleCase(string input)
        {
            string output = txtInfo.ToTitleCase(input);
            output = output.Replace(" At ", " at ", StringComparison.Ordinal)
                           .Replace(" In ", " in ", StringComparison.Ordinal)
                           .Replace(" And ", " and ", StringComparison.Ordinal)
                           .Replace(" Of ", " of ", StringComparison.Ordinal)
                           .Replace(" For ", " for ", StringComparison.Ordinal)
                           .Replace(" In ", " in ", StringComparison.Ordinal)
                           .Replace(" A ", " a ", StringComparison.Ordinal)
                           .Replace(" An ", " an ", StringComparison.Ordinal)
                           .Replace(" And ", " and ", StringComparison.Ordinal)
                           .Replace(" To ", " to ", StringComparison.Ordinal)
                           .Replace(" On ", " on ", StringComparison.Ordinal)
                           .Replace(" Or ", " or ", StringComparison.Ordinal)
                           .Replace(" As ", " as ", StringComparison.Ordinal)
                           .Replace(" Is ", " is ", StringComparison.Ordinal)
                           .Replace(" No ", " no ", StringComparison.Ordinal)
                           .Replace(" Uk ", " UK ", StringComparison.Ordinal)
                           .Replace(" Usa ", "USA", StringComparison.Ordinal)
                           .Replace("Wwi ", "WWI", StringComparison.Ordinal)
                           .Replace("Ww I", "WW I", StringComparison.Ordinal)
                           .Replace("Wwii ", "WWII", StringComparison.Ordinal)
                           .Replace("Ww Ii ", "WW II", StringComparison.Ordinal)
                           .Replace("1St", "1st", StringComparison.Ordinal)
                           .Replace("1Th", "1th", StringComparison.Ordinal) // 11th
                           .Replace("2Nd", "2nd", StringComparison.Ordinal)
                           .Replace("2Th", "2th", StringComparison.Ordinal) // 12th
                           .Replace("3Rd", "3rd", StringComparison.Ordinal)
                           .Replace("3Th", "3th", StringComparison.Ordinal) // 13th
                           .Replace("4Th", "4th", StringComparison.Ordinal)
                           .Replace("5Th", "5th", StringComparison.Ordinal)
                           .Replace("6Th", "6th", StringComparison.Ordinal)
                           .Replace("7Th", "7th", StringComparison.Ordinal)
                           .Replace("8Th", "8th", StringComparison.Ordinal)
                           .Replace("9Th", "9th", StringComparison.Ordinal)
                           .Replace("0Th", "0th", StringComparison.Ordinal);
            return output;
        }

        public static string ConvertStringArrayToString(string[] array)
        {
            char[] charsToTrim = [',', '.', ' '];
            StringBuilder builder = new();
            foreach (string value in array)
            {
                builder.Append(value);
                builder.Append(", ");
            }
            return builder.ToString().TrimEnd(charsToTrim);
        }

        public static string RemoveSupriousDateCharacters(string text)
        {
            StringBuilder sb = new();
            text ??= string.Empty;
            foreach (char ch in text)
            {
                // En Dash(150, 8211), or Em Dash(151, 8212)
                if (ch == 150 || ch== 151 || ch == '-' || ch == 8211 || ch == 8212)
                    sb.Append('-');
                else if (ch >= ' ' && ch <= 'Z')
                    sb.Append(ch);
                else
                    sb.Append(' ');
            }
            return sb.ToString().ClearWhiteSpace();
        }

        public static string RemoveDiacritics(string text)
        {
            if (text is null) return string.Empty;
            if (!FileHandling.Default.ConvertDiacritics)
                return text; // only process if user wants to remove diacrits
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ClearWhiteSpace(this string text) =>
            text.Replace(Environment.NewLine, " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal)
                .Replace(";", " ", StringComparison.Ordinal).Replace(":", " ", StringComparison.Ordinal).Replace("~", " ", StringComparison.Ordinal)
                .Replace("\t", " ", StringComparison.Ordinal).Replace("   ", " ", StringComparison.Ordinal).Replace("   ", " ", StringComparison.Ordinal)
                .Replace("  ", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal)
                .Replace("  ", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal)
                .Trim();

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.AsSpan(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.AsSpan(previousIndex));

            return sb.ToString();
        }
    }
}
