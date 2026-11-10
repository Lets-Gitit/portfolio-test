using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private VNScriptData[] scriptDatas;

    [SerializeField] private DirectionManager directionManager;
    [SerializeField] private DialogueUIManager dialogueUIManager;
    [SerializeField] private IllustManager illustManager;
    [SerializeField] private SoundManager soundManager;

    private Dictionary<CommandType, Func<ScriptEntry, IEnumerator>> scriptCommandDispatcher;

    private void Awake() {
        scriptCommandDispatcher = new() {
            [CommandType.None] = e => null,
            [CommandType.Dialogue] = e => dialogueUIManager.RunDialogue(e),
            [CommandType.IllustIn] = e => illustManager.IllustIn(e),
            [CommandType.IllustOut] = e => illustManager.IllustOut(e),
            [CommandType.ChangeExpression] = e => illustManager.ChangeExpression(e),
            [CommandType.PlayBGM] = e => { soundManager.PlayBGM(e); return null; },
            [CommandType.PlaySFX] = e => soundManager.PlaySFX(e),
            [CommandType.ShowBG] = e => { directionManager.ShowBG(e.fileName); return null; },
            [CommandType.Direction] = e => directionManager.Direction(e)
        };

        foreach (CommandType t in Enum.GetValues(typeof(CommandType))) {
            if (!scriptCommandDispatcher.ContainsKey(t)) {
                Debug.LogWarning($"{t}은(는) 디스패처로 처리되지 않았음.");
            }
        }
    }

    private IEnumerator Start() {
        foreach (var scriptData in scriptDatas) {
            foreach (var entry in scriptData.entries) {
                if (scriptCommandDispatcher.TryGetValue(entry.commandType, out var run)) {
                    var co = run(entry);
                    if (co != null) yield return co;
                }
                else {
                    Debug.LogWarning($"존재하지 않는 메서드 : {entry.commandType}");
                }

                yield return null;
            }
        }
    }
}

