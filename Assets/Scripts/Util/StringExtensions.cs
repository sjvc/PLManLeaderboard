using System;

public static class StringExtensions {

    public static String Substring(this string str, string startStr, string endStr, bool includeStart = true, bool includeEnd = true) {
        if (!str.Contains(startStr) || !str.Contains(endStr)) {
            return "";
        }

        string result = str.Substring( str.IndexOf(startStr) + (includeStart ? 0 : startStr.Length) );
        result = result.Substring(0, result.IndexOf(endStr) + (includeEnd ? endStr.Length : 0));

        return result;
    }
    
}