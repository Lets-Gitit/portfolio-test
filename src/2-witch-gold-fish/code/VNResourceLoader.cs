using System.Collections.Generic;
using UnityEngine;

public class VNResourceLoader
{
    private static readonly Dictionary<string, Object> _cache = new();

    public static T Load<T>(string path, bool useCache = true)
        where T : Object {
        if (useCache && _cache.TryGetValue(path, out var obj)) {
            if (obj) {
                return obj as T;
            }
            _cache.Remove(path);
        }
           
        var loaded = Resources.Load<T>(path);

        if (loaded == null) {
            Debug.LogWarning($"[VNResourceLoader] Missing resource: {path}");
            return null;
        }

        if (useCache) _cache[path] = loaded;

        return loaded;
    }

    public static void ClearCache() => _cache.Clear();

    public static Sprite LoadCharacterSprite(string fileName, string expression = "") {
        var name = $"{fileName}{expression}";
        var path = VNConstants.Join(VNConstants.Paths.ArtCharacter, name);

        return Load<Sprite>(path);
    }

    public static Sprite LoadBG(string fileName) {
        var path = VNConstants.Join(VNConstants.Paths.ArtBG, fileName);

        return Load<Sprite>(path);
    }

    public static AudioClip LoadSoundByType(FileType type, string fileName) {
        string basePath = 
            (type == FileType.BGM)
            ? VNConstants.Paths.SoundBGM
            : VNConstants.Paths.SoundSFX;

        var path = VNConstants.Join(basePath, fileName);
        return Load<AudioClip>(path);
    }
}
