using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DirectionManager : MonoBehaviour
{
    [SerializeField] private Transform UIRootTransform;
    [SerializeField] private Image background;

    [SerializeField] private TMP_Text captionText;
    [SerializeField] private CanvasGroup captionGroup;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private AudioSource textBlip;

    [SerializeField] private Animator fade;
    [SerializeField] private Animator circleWipe;

    [SerializeField] private Volume volume;
    [SerializeField] private FilterPresetAsset filterAsset;

    private float directionDelay = 1f;
    private float captionDelay = 2f;

    public IEnumerator Direction(ScriptEntry entry) {
        switch (entry.stageDirection) {
            case StageDirection.CameraShake:
                StartCoroutine(CameraShake(30f, 0.4f));
                yield break;
            case StageDirection.Caption:
                yield return Caption(entry);
                break;
            case StageDirection.CaptionDialogue:
                yield return CaptionDialogue(entry);
                break;
            case StageDirection.FadeIn:
                yield return PlayDirections(fade, "FadeIn");
                break;
            case StageDirection.FadeOut:
                yield return PlayDirections(fade, "FadeOut");
                break;
            case StageDirection.CircleWipeIn:
                yield return PlayDirections(circleWipe, "WipeIn");
                break;
            case StageDirection.CircleWipeOut:
                yield return PlayDirections(circleWipe, "WipeOut");
                break;
            case StageDirection.FilterIn:
                yield return ApplyFilter(entry.caption);
                break;
            case StageDirection.FilterOut:
                yield return ApplyFilter("Normal");
                break;
        }
    }

    public void ShowBG(string fileName) {
        background.sprite = VNResourceLoader.LoadBG(fileName);
    }

    private IEnumerator CameraShake(float magnitude, float duration) {
        var rt = UIRootTransform;
        Vector2 orig = rt.localPosition;
        float phaseDur = duration / 5f;

        Vector2 rightPos = orig + Vector2.right * magnitude;
        Vector2 leftPos = orig + Vector2.left * magnitude;

        yield return LerpUtil.Lerp(orig, rightPos, phaseDur, v => rt.localPosition = v);
        yield return LerpUtil.Lerp(rightPos, leftPos, phaseDur, v => rt.localPosition = v);
        yield return LerpUtil.Lerp(leftPos, rightPos, phaseDur, v => rt.localPosition = v);
        yield return LerpUtil.Lerp(rightPos, leftPos, phaseDur, v => rt.localPosition = v);
        yield return LerpUtil.Lerp(leftPos, orig, phaseDur, v => rt.localPosition = v);

        rt.localPosition = orig;
    }

    private IEnumerator Caption(ScriptEntry entry) {
        captionText.gameObject.SetActive(true);
        captionText.text = entry.caption;

        captionDelay = (entry.metaDataKey == "long") ? 5f : 2f;

        yield return StartCoroutine(FadeCanvasGroup(captionGroup, 0f, 1f, 0.3f));
        yield return new WaitForSeconds(captionDelay);
        yield return StartCoroutine(FadeCanvasGroup(captionGroup, 1f, 0f, 0.3f));
        captionText.gameObject.SetActive(false);
    }

    private IEnumerator CaptionDialogue(ScriptEntry entry) {
        captionGroup.alpha = 1f;

        yield return VNTextTyper.TypeText(dialogueText, entry.caption,
            new VNTextTypingMetaData {
            blip = textBlip,
            blipInterval = 3,
            charDelay = 0.04f,
            parseStyleTags = false,
            prefixTag = "",
            postfixTag = ""
        });

        if (entry.waitForInput) {
            yield return new WaitUntil(() => {
                return Keyboard.current.spaceKey.wasPressedThisFrame;
            });
        }

        dialogueText.text = "";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration) {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator PlayDirections(Animator anim, string trigger) {
        anim.SetTrigger(trigger);
        yield return new WaitForSeconds(directionDelay);
    }

    private IEnumerator ApplyFilter(string key) {
        bool isValid = filterAsset.TryGetPreset(key, out var target);

        if (volume.profile.TryGet<ColorAdjustments>(out var adj) && isValid) {
            var final = target;
            yield return LerpUtil.Lerp(adj.postExposure.value, target.postExposure, 0.125f, v => adj.postExposure.value = v);
            yield return LerpUtil.Lerp(adj.saturation.value, target.saturation, 0.125f, v => adj.saturation.value = v);
            yield return LerpUtil.Lerp(adj.contrast.value, target.contrast, 0.125f, v => adj.contrast.value = v);
            yield return LerpUtil.Lerp(adj.colorFilter.value, target.colorFilter, 0.125f, v => adj.colorFilter.value = v);
        }
    }
}

