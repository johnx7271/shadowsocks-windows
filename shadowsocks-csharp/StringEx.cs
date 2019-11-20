using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

#if EXPOSE_EVERYTHING || EXPOSE_STRINGEX
public
#endif
static partial class StringEx
{
#pragma warning disable 1591

	#region basic String methods

	public static bool IsNullOrEmpty(this string value)
		=> string.IsNullOrEmpty(value);

	public static bool IsNullOrWhiteSpace(this string value)
		=> string.IsNullOrWhiteSpace(value);

	public static bool IsWhiteSpace(this string value)
	{
		foreach (var c in value)
		{
			if (char.IsWhiteSpace(c)) continue;

			return false;
		}
		return true;
	}

	#endregion

	#region BeginWith

	public static bool BeginWithAny(this string s, IEnumerable<char> chars)
	{
		if (s.IsNullOrEmpty()) return false;
		return chars.Contains(s[0]);
	}

	public static bool BeginWith(this string a, string b, StringComparison comparisonType)
	{
		if (a == null || b == null) return false;

		return a.StartsWith(b, comparisonType);
	}

	#endregion

	#region ToLines

	public static IEnumerable<string> NonWhiteSpaceLines(this TextReader reader)
	{
		string line;
		while ((line = reader.ReadLine()) != null)
		{
			if (line.IsWhiteSpace()) continue;
			yield return line;
		}
	}

	#endregion
}