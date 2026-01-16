using System.Text.RegularExpressions;

namespace FTAnalyzer.Utilities
{
    internal static partial class RegexPatterns
    {
        const string CHILDREN_STATUS_PATTERN1 = @"(\d{1,2}) Total ?,? ?(\d{1,2}) (Alive|Living) ?,? ?(\d{1,2}) Dead";
        const string CHILDREN_STATUS_PATTERN2 = @"Total:? (\d{1,2}) ?,? ?(Alive|Living):? (\d{1,2}) ?,? ?Dead:? (\d{1,2})";
        const string AGE_PATTERN = @"^(?<year>\d{1,3}y)? ?(?<month>\d{1,2}m)? ?(?<day>\d{1,2}d)?$";

        [GeneratedRegex(CHILDREN_STATUS_PATTERN1, RegexOptions.Compiled)]
        public static partial Regex ChildrenStatusPattern1();
        [GeneratedRegex(CHILDREN_STATUS_PATTERN2, RegexOptions.Compiled)]
        public static partial Regex ChildrenStatusPattern2();

        [GeneratedRegex(AGE_PATTERN, RegexOptions.Compiled)]
        public static partial Regex AgeRegex();
    }
}
