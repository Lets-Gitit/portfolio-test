using System.Collections.Generic;

public static class VNTextParser
{
    public static List<string> SplitByDelay(string text) {
        return new List<string>(text.Split(new[] { "<delay>" }, System.StringSplitOptions.None));
    }

    public static (string prefix, string postfix) GetTextStyleTag(TextType textType) {
        return textType switch {
            TextType.Normal => ("", ""),
            TextType.Small => ("<size=80%>", "</size>"),
            TextType.Gray => ("<color=#A5C6A9>", "</color>"),
            _ => ("", "")
        };
    }
}
