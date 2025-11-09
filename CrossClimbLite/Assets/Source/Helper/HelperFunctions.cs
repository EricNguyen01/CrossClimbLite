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
    }
}
