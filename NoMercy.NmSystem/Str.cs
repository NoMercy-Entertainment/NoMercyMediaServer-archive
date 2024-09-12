using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;

namespace NoMercy.NmSystem;

public static partial class Str
{
    public static double MatchPercentage(string strA, string strB)
    {
        int result = 0;
        for (int i = strA.Length - 1; i >= 0; i--)
            if (i >= strB.Length || strA[i] == strB[i])
            {
                // Do nothing
            }
            else if (char.ToLower(strA[i]) == char.ToLower(strB[i]))
            {
                result += 1;
            }
            else
            {
                result += 4;
            }

        return 100 - (result + 4 * Math.Abs(strA.Length - strB.Length)) / (2.0 * (strA.Length + strB.Length)) * 100;
    }

    public static List<T> SortByMatchPercentage<T>(IEnumerable<T> array, Func<T, string> keySelector, string match)
    {
        return array.OrderBy(item => MatchPercentage(match, keySelector(item))).ToList();
    }

    public static string RemoveAccents(this string s)
    {
        Encoding destEncoding = Encoding.GetEncoding("ISO-8859-1");

        return destEncoding.GetString(
            Encoding.Convert(Encoding.UTF8, destEncoding, Encoding.UTF8.GetBytes(s)));
    }

    public static string RemoveDiacritics(this string text)
    {
        string formD = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new();

        foreach (char ch in formD)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string RemoveNonAlphaNumericCharacters(this string text)
    {
        return Regex.Replace(text, @"[^a-zA-Z0-9\s.-]", "");
    }

    [GeneratedRegex(@"(1(8|9)|20)\d{2}(?!p|i|(1(8|9)|20)\d{2}|\W(1(8|9)|20)\d{2})")]
    public static partial Regex MatchYearRegex();

    public static string PathName(this string path)
    {
        return Regex.Replace(path, @"[\/\\]", Path.DirectorySeparatorChar.ToString());
    }

    public static int ToInt(this string path)
    {
        if (string.IsNullOrEmpty(path)) return 0;
        return int.Parse(path);
    }

    public static int ToInt(this double value)
    {
        return Convert.ToInt32(value);
    }

    public static string Spacer(string text, int padding, bool begin = false)
    {
        return begin ? SpacerBegin(text, padding) : SpacerEnd(text, padding);
    }

    private static string SpacerEnd(string text, int padding)
    {
        StringBuilder spacing = new();
        spacing.Append(text);
        for (int i = 0; i < padding - text.Length; i++) spacing.Append(' ');

        return spacing.ToString();
    }

    private static string SpacerBegin(string text, int padding)
    {
        StringBuilder spacing = new();
        for (int i = 0; i < padding - text.Length; i++) spacing.Append(' ');
        spacing.Append(text);

        return spacing.ToString();
    }

    public static string ToHexString(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static string ToHexString(this Rgb24 color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static Guid ToGuid(this string id)
    {
        return Guid.Parse(id);
    }

    public static string ToUtf8(this string value)
    {
        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(value));
    }

    public static string SplitPascalCase(this string str)
    {
        str = Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2");
        return Regex.Replace(str, @"(\p{Ll})(\P{Ll})", "$1 $2");
    }
    
    /** This method is used to sanitize a string by removing diacritics, non-alphanumeric characters and accents. */
    public static string Sanitize(this string str)
    {
        return str.RemoveDiacritics().RemoveNonAlphaNumericCharacters().RemoveAccents();
    }
    
    public static bool ContainsSanitized(this string str, string value)
    {
        str = str.Sanitize();
        value = value.Sanitize();
        return str.Contains(value) || value.Contains(str);
    }
}