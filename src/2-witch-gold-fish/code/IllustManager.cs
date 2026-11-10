using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IllustMoveDirection { Up, Down }
public enum FadeOption { In, Out }

public class IllustManager : MonoBehaviour
{
    [SerializeField] private IllustrationMetaDataAsset dataAsset;
    [SerializeField] private Transform parent;
    [SerializeField] private Image characterImageTemplate;

    private Dictionary<string, ImageData> spawnedImages = new Dictionary<string, ImageData>();
    private List<Image> characterList = new List<Image>();
    private float fadeDuration = 0.2f;

    public IEnumerator IllustIn(ScriptEntry entry) {
        var instance = Instantiate(characterImageTemplate.gameObject, parent);
        var img = instance.GetComponent<Image>();
        var metaData = dataAsset.GetIllustrationMetaData(entry.metaDataKey);

        img.sprite = LoadCharacterSprite(entry.fileName, entry.expression);
        characterList.Add(img);

        spawnedImages[entry.fileName] = new ImageData {img = img, metaDataKey = entry.metaDataKey};

        ApplyMetaDataTransform(img, metaData);

        yield return FadeImage(img, FadeOption.In, 0.7f);
    }

    public IEnumerator IllustOut(ScriptEntry entry) {
        if(!spawnedImages.TryGetValue(entry.fileName, out var imageData)) {
            Debug.LogWarning($"{entry.fileName}가 존재하지 않습니다.");
            yield break;
        }

        yield return FadeImage(imageData.img, FadeOption.Out, 0.7f);
        spawnedImages.Remove(entry.fileName);
        Destroy(imageData.img.gameObject);
    }

    public IEnumerator ChangeExpression(ScriptEntry entry) {
        if(!spawnedImages.TryGetValue(entry.fileName, out var imageData)) {
            Debug.LogWarning($"{entry.fileName}가 존재하지 않음.");
            yield break;
        }  

        var currentKey = imageData.metaDataKey;
        var selectedKey = ChooseMetaKey(entry.metaDataKey, imageData.metaDataKey);
        var meta = dataAsset.GetIllustrationMetaData(selectedKey);

        if(selectedKey == currentKey) {
            imageData.img.sprite = LoadCharacterSprite(entry.fileName, entry.expression);
            yield break;
        }

        yield return FadeImage(imageData.img, FadeOption.Out, fadeDuration);

        imageData.img.sprite = LoadCharacterSprite(entry.fileName, entry.expression);
        ApplyMetaDataTransform(imageData.img, meta);

        yield return FadeImage(imageData.img, FadeOption.In, fadeDuration);

        imageData.metaDataKey = selectedKey;
    }

    private string ChooseMetaKey(string requestedKey, string currentKey) {
        if (string.IsNullOrEmpty(requestedKey)) {
            return currentKey;
        }

        var metaData = dataAsset.GetIllustrationMetaData(requestedKey);
        if (metaData != null) {
            return requestedKey;
        }

        Debug.LogWarning($"메타 데이터 키'{requestedKey}'가 존재하지 않음. 기존 키 '{currentKey}'사용");
        return currentKey;
    }

    private IEnumerator FadeImage(Image img, FadeOption opt, float duration) {
        float from = opt == FadeOption.In ? 0f : 1f;
        float to = opt == FadeOption.In ? 1f : 0f;

        yield return LerpUtil.Lerp(from, to, duration, a => {
            var c = img.color;
            c.a = a;
            img.color = c;
        });
    }
    
    private Sprite LoadCharacterSprite(string fileName, string expression) {
        return VNResourceLoader.LoadCharacterSprite(fileName, expression);
    }

    private void ApplyMetaDataTransform(Image img, IllustrationMetaData metaData) {
        var rt = img.rectTransform;
        rt.sizeDelta = metaData.size;
        rt.anchoredPosition = metaData.pos;
    }

    private IEnumerator MoveSprite(IllustrationMetaData data, Image img, IllustMoveDirection dir ,float duration = 0.7f) {
        var rt = img.rectTransform;

        float canvasHeight = ((RectTransform)rt.root).rect.height * 0.5f;

        Vector2 offscreenPos = dir == IllustMoveDirection.Up
            ? new Vector2(data.pos.x, -canvasHeight - data.size.y * 0.7f)
            : new Vector2(data.pos.x, -(canvasHeight + data.size.y * 0.7f));

        Vector2 from = dir == IllustMoveDirection.Up ? offscreenPos : data.pos;
        Vector2 to   = dir == IllustMoveDirection.Up ? data.pos : offscreenPos;

        yield return LerpUtil.Lerp(from, to, duration, v => rt.anchoredPosition = v);
    }
}

public class ImageData
{
    public Image img;
    public string metaDataKey;
}