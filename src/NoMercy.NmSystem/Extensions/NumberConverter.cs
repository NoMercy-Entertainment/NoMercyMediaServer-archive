namespace NoMercy.NmSystem.Extensions;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public partial class NumberConverter
{
    private static readonly Dictionary<int, string> UnitsMap = new()
    {
        { 0, "zero" }, { 1, "one" }, { 2, "two" }, { 3, "three" }, { 4, "four" },
        { 5, "five" }, { 6, "six" }, { 7, "seven" }, { 8, "eight" }, { 9, "nine" },
        { 10, "ten" }, { 11, "eleven" }, { 12, "twelve" }, { 13, "thirteen" },
        { 14, "fourteen" }, { 15, "fifteen" }, { 16, "sixteen" }, { 17, "seventeen" },
        { 18, "eighteen" }, { 19, "nineteen" }
    };

    private static readonly Dictionary<int, string> TensMap = new()
    {
        { 2, "twenty" }, { 3, "thirty" }, { 4, "forty" }, { 5, "fifty" },
        { 6, "sixty" }, { 7, "seventy" }, { 8, "eighty" }, { 9, "ninety" }
    };

    private static string NumberToWords(int number)
    {
        return number switch
        {
            0 => "zero",
            < 0 => "minus " + NumberToWords(Math.Abs(number)),
            _ => NumberToWordsRecursive(number)
        };
    }

    private static string NumberToWordsRecursive(int number)
    {
        return number switch
        {
            < 20 => UnitsMap[number],
            < 100 => TensMap[number / 10] + (number % 10 != 0 ? " " + UnitsMap[number % 10] : ""),
            < 1000 => UnitsMap[number / 100] + " hundred" +
                      (number % 100 != 0 ? " " + NumberToWordsRecursive(number % 100) : ""),
            < 1000000 => NumberToWordsRecursive(number / 1000) + " thousand" +
                         (number % 1000 != 0 ? " " + NumberToWordsRecursive(number % 1000) : ""),
            < 1000000000 => NumberToWordsRecursive(number / 1000000) + " million" +
                            (number % 1000000 != 0 ? " " + NumberToWordsRecursive(number % 1000000) : ""),
            _ => NumberToWordsRecursive(number / 1000000000) + " billion" +
                 (number % 1000000000 != 0 ? " " + NumberToWordsRecursive(number % 1000000000) : "")
        };
    }

    internal static string ConvertNumbersInString(string input)
    {
        return MyRegex().Replace(input, match => NumberToWords(int.Parse(match.Value)));
    }

    [GeneratedRegex(@"\b\d+\b")]
    private static partial Regex MyRegex();
}