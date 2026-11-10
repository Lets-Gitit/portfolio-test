using System.Collections;
using TMPro;
using UnityEngine;

public static class VNTextTyper
{
    private const float pauseDelay = 0.5f;

    public static IEnumerator TypeText(TMP_Text target, string text, VNTextTypingMetaData meta) {
        meta ??= VNTextTypingMetaData.Default;
        var parts = VNTextParser.SplitByDelay(text);
        target.text = meta.prefixTag;

        int count = 0;

        foreach (var part in parts) {
            for(int i = 0; i < part.Length; i++) {
                if(meta.parseStyleTags && part[i] == '<') {
                    int close = part.IndexOf('>', i);
                    if(close != -1) {
                        target.text += part.Substring(i, close - i + 1);
                        i = close;
                        continue;
                    }
                }

                if (part[i] == '\\' && i + 1 < part.Length && part[i + 1] == 'n') {
                    target.text += '\n';
                    i++;
                    continue;
                }

                target.text += part[i];

                if(++count > meta.blipInterval) {
                    meta.blip?.Play();
                    count = 0;
                }
                // 글자 출력 딜레이
                yield return new WaitForSeconds(meta.charDelay);
            }
            // <delay> 태그 딜레이
            yield return new WaitForSeconds(pauseDelay);
        }

        target.text += meta.postfixTag;
    }
}
