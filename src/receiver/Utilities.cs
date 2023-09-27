using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    public class Utilities
    {
        static public int Map(int sourceMin, int sourceMax, int targetMin, int targetMax, int value)
        {
            // Ensure the input value is within the source range
            value = Math.Max(sourceMin, Math.Min(sourceMax, value));

            // Calculate the percentage of the input value within the source range
            int percentage = (value - sourceMin) / (sourceMax - sourceMin);

            // Map the percentage to the target range
            int mappedValue = targetMin + percentage * (targetMax - targetMin);

            return mappedValue;
        }
    }
}
