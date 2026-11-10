using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private VNSoundDataAsset soundData;

    private float BGMVolume = 0.4f;
    private float SFXVolume = 0.4f;

    public bool TryPrepareClip(ScriptEntry entry, out AudioClip clip, 
        out float volume, out bool waitComplete, bool isBGM) 
    {
        clip = VNResourceLoader.LoadSoundByType(entry.fileType, entry.fileName);

        if (clip == null) {
            Debug.LogWarning($"{(isBGM?"BGM":"SFX")} clip이 존재하지 않음.");
            volume = 0f;
            waitComplete = false;
            return false;
        }

        volume = isBGM ? BGMVolume : SFXVolume;
        waitComplete = false;

        if (soundData.TryGet(entry.fileName, out var data)) {
            volume = data.volume;
            waitComplete = data.waitComplete;
        }

        return true;
    }

    public void PlayBGM(ScriptEntry entry) {
        TryPrepareClip(entry, out var clip, out var vol, out var wait, true);

        bgmSource.clip = clip;
        bgmSource.volume = vol;
        bgmSource.Play();       
    }

    public IEnumerator PlaySFX(ScriptEntry entry) {
        if (!TryPrepareClip(entry, out var clip, out var vol, out var wait, false)) {
            yield break;
        }

        sfxSource.PlayOneShot(clip, vol);

        if (wait) {
            yield return new WaitForSeconds(clip.length);
        }
    }
}