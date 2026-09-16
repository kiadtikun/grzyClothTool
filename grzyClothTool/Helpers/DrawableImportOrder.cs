using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace grzyClothTool.Helpers;

public static class DrawableImportOrder
{
    private static readonly Regex PedPrefix = new(@"^mp_[mf]_freemode_01(?:_p)?(?:_|$)", RegexOptions.IgnoreCase);
    private static readonly IComparer<string> DlcComparer = Comparer<string>.Create(CompareDlcNames);

    public static string GetDlcName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var separator = name.IndexOf('^');
        return separator < 0 ? string.Empty : PedPrefix.Replace(name[..separator], string.Empty);
    }

    public static string[] OrderFiles(IEnumerable<string> paths) => paths
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(path => string.IsNullOrEmpty(GetDlcName(path)))
        .ThenBy(GetDlcName, DlcComparer)
        .ThenBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
        .ThenBy(path => path, StringComparer.Ordinal)
        .ToArray();

    // Compare numeric DLC suffixes without integer overflow; keep padding deterministic.
    private static int CompareDlcNames(string left, string right)
    {
        var a = Regex.Split(left, "([0-9]+)");
        var b = Regex.Split(right, "([0-9]+)");
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            int result;
            if (i % 2 == 1)
            {
                var x = a[i].TrimStart('0');
                var y = b[i].TrimStart('0');
                result = x.Length.CompareTo(y.Length);
                if (result == 0) result = StringComparer.Ordinal.Compare(x, y);
            }
            else result = StringComparer.OrdinalIgnoreCase.Compare(a[i], b[i]);
            if (result != 0) return result;
        }
        var lengthComparison = a.Length.CompareTo(b.Length);
        return lengthComparison != 0 ? lengthComparison : StringComparer.OrdinalIgnoreCase.Compare(left, right);
    }
}
