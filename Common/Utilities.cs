using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Juxce.Tuneage.Common
{
  public class Utilities
  {
    public static string GetTicks()
    {
      return DateTime.UtcNow.Ticks.ToString("d20");
    }

    public static string MakeSearchString(string source)
    {
      StringBuilder sb = new StringBuilder();
      char[] sourceChars = source.ToCharArray();

      foreach (char c in sourceChars)
      {
        if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
        {
          sb.Append(c);
        }
      }

      return sb.ToString().ToLower();
    }

    public static string SanitizePrimaryKey(string source)
    {
      string pattern = @"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]";
      Regex DisallowedCharsInTableKeys = new Regex(pattern);
      bool invalidKey = DisallowedCharsInTableKeys.IsMatch(source);

      if (invalidKey)
      {
        return DisallowedCharsInTableKeys.Replace(source, "");
      }
      else
      {
        return source;
      }
    }
  }
}