using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractBingMapsCollectionUtilities
{
    public static class CsvEscape
    {
        public static string Escape(string input)
        {
            if (input.Contains('"') || input.Contains(',') || input.Contains('\n') || input.Contains('\r'))
            {
                // Escape double quotes by doubling them
                var escaped = input.Replace("\"", "\"\"");
                // Wrap the entire string in double quotes
                return $"\"{escaped}\"";
            }
            return input;
        }
    }
}
