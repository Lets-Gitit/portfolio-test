using UnityEngine;

public enum CommandType { None = 0, Dialogue, IllustIn, IllustOut, ChangeExpression, PlayBGM, PlaySFX, ShowBG, Direction }
public enum StageDirection { None = 0, FadeOut , FadeIn, CircleWipeOut, CircleWipeIn, CameraShake, Caption, CaptionDialogue, FilterIn, FilterOut }
public enum FileType { None = 0, BGM, SFX }
public enum TextType { None = 0, Normal, Gray, Small }

[CreateAssetMenu(fileName = "VNScriptData", menuName = "VN/Script Data")]
public class VNScriptData : ScriptableObject
{
    public ScriptEntry[] entries;
}

[System.Serializable]
public class ScriptEntry
{
    public CommandType commandType;
    public string characterName;
    [TextArea] public string text;
    public TextType textType;
    public bool waitForInput;
    public string fileName;
    public string expression;
    public string metaDataKey;
    public FileType fileType;
    public StageDirection stageDirection;
    public string caption;
}