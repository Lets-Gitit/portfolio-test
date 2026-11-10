using System;
using System.Collections;
using UnityEngine;

public static class LerpUtil
{
    public static IEnumerator Lerp(float from, float to, float duration, Action<float> setter) {
        float elapsed = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            setter(Mathf.Lerp(from, to, t));
            yield return null;
        }
        setter(to);
    }

    public static IEnumerator Lerp(Vector2 from, Vector2 to, float duration, Action<Vector2> setter) {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            setter(Vector2.Lerp(from, to, t));
            yield return null;
        }
        setter(to);
    }

    public static IEnumerator Lerp(Color from, Color to, float duration, Action<Color> setter) {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            setter(Color.Lerp(from, to, t));
            yield return null;
        }
        setter(to);
    }
}
