using System;
using UnityEngine;

namespace CrossClimbLite
{
    public static class HelperFunctions
    {
        public static string FormatSecondsToString(float seconds)
        {
            if (seconds < 60f)
            {
                return $"{MathF.Floor(seconds)}s";
            }
            else if (seconds < 3600f)
            {
                int minutes = (int)(seconds / 60f); 

                int remainingSeconds = (int)(seconds % 60f);

                return $"{minutes}m{remainingSeconds}s";
            }
            else
            {
                return "1h+";
            }
        }

        public static int GetWordCountFromString(string input)
        {
            // Split on any whitespace, remove empty entries
            string[] words = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Join them back with a single space
            string cleaned = string.Join(" ", words);

            return words.Length;
        }
    }
}
