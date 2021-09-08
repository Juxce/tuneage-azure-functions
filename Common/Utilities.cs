using System;

namespace Juxce.Tuneage.Common {
    public class Utilities {
        public static string GetTicks() {
            return DateTime.UtcNow.Ticks.ToString("d20");
        }
    }
}