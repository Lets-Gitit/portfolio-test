using UnityEngine;

public static class VNConstants
{
    public static class Paths {
        public const string ArtCharacter = "Art/VN/Character/";
        public const string ArtBG = "Art/VN/BackGround/";
        public const string SoundRoot = "Sound/";
        public const string SoundBGM = "Sound/BGM/";
        public const string SoundSFX = "Sound/SFX/";
    }

    public static string Join(string a, string b)
        => $"{a.TrimEnd('/')}/{b.TrimStart('/')}";

    public static string Join(string a, string b, string c)
        => Join(Join(a, b), c);
}
