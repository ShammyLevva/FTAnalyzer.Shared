using System;
using System.Collections.Generic;
using System.Reflection;

namespace FTAnalyzer.Utilities
{
    public static partial class ExtensionMethods
    {
#if __PC__
        public static void DoubleBuffered(this System.Windows.Forms.DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
#endif
        public static string Right(this string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            //Return the string
            return sValue;
        }

        public static string Between(this string value, char startChar, char endChar)
        {
            int pos1 = value.IndexOf(startChar) + 1;
            int pos2 = value.IndexOf(endChar);
            if (pos1 == -1 || pos2 == -1 || pos1 >= pos2)
                return string.Empty;
            return value[pos1..pos2];
        }

        public static DateTime TryAddYears(this DateTime date, int years)
        {
            // Make sure adding/subtracting years won't put date 
            // over DateTime.MaxValue or below DateTime.MinValue
            if ((date == FactDate.MINDATE && years < 0) || (date == FactDate.MAXDATE && years > 0)) return date;
            try
            {
                date = date.AddYears(years);
            }
            catch (ArgumentOutOfRangeException)
            {
                date = years >= 0 ? FactDate.MAXDATE : FactDate.MINDATE;
            }
            if (date > FactDate.MAXDATE) date = FactDate.MAXDATE;
            if (date < FactDate.MINDATE) date = FactDate.MINDATE;
            return date;
        }

        public static bool StartsWithNumeric(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            char first = input[0];
            return first == '0' || first == '1' || first == '2' || first == '3' || first == '4' || first == '5' || first == '6' || first == '7' || first == '8' || first == '9';
        }

        public static bool IsNumeric(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return int.TryParse(input, out _);
        }

        public static bool ContainsString(this List<string> list, string str)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == str)
                        return true;
            }
            return false;
        }

        public static bool ContainsString(this IList<string> list, string str)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == str)
                        return true;
            }
            return false;
        }

        public static bool ContainsFact(this IList<Fact> list, Fact fact)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == fact)
                        return true;
            }
            return false;
        }
        public static bool ContainsLocation(this IList<FactLocation> list, FactLocation loc)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == loc)
                        return true;
            }
            return false;
        }
        public static bool ContainsLocation(this List<IDisplayLocation> list, IDisplayLocation loc)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == loc)
                        return true;
            }
            return false;
        }
        public static bool ContainsFact(this List<Fact> list, Fact fact)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == fact)
                        return true;
            }
            return false;
        }
        public static bool ContainsFact(this SortableBindingList<IDisplayFact> list, IDisplayFact fact)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == fact)
                        return true;
            }
            return false;
        }
        public static bool ContainsDuplicate(this SortableBindingList<IDisplayDuplicateIndividual> list, IDisplayDuplicateIndividual dup)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == dup)
                        return true;
            }
            return false;
        }
        public static bool ContainsDuplicate(this IList<NonDuplicate> list, NonDuplicate dup)
        {
            if (list is not null)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == dup)
                        return true;
            }
            return false;
        }

        public static void AddUnique<T>(this IList<T> self, IEnumerable<T> items)
        {
            foreach (var item in items)
                if (!self.Contains(item))
                    self.Add(item);
        }
    }
}
