using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private AudioSource textBlipSFX;
    [SerializeField] private GameObject next;

    private string currentCharacter = null;
    private TextType currentTextType = TextType.None;

    public IEnumerator RunDialogue(ScriptEntry entry) {
        if (!string.IsNullOrEmpty(entry.characterName) && entry.characterName != currentCharacter) { 
            currentCharacter = entry.characterName;
            characterName.text = currentCharacter;
        }

        if(entry.textType != TextType.None && entry.textType != currentTextType) {
            currentTextType = entry.textType;
        }

        var (prefixTag, postfixTag) = VNTextParser.GetTextStyleTag(entry.textType);

        var meta = new VNTextTypingMetaData {
            blip = textBlipSFX,
            blipInterval = 2,
            charDelay = 0.04f,
            prefixTag = prefixTag,
            postfixTag = postfixTag
        };

        yield return VNTextTyper.TypeText(contentText, entry.text, meta);

        yield return new WaitForSeconds(0.3f);
        next.gameObject.SetActive(true);

        if(entry.waitForInput) {
            yield return new WaitUntil(() => {
                return Keyboard.current.spaceKey.wasPressedThisFrame;
            });
        }

        next.gameObject.SetActive(false);
    }
}
