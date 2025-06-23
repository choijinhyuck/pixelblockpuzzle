using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundClip
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    // playLength: 0이면 AudioClip의 전체 길이를 재생, 0보다 크면 해당 시간 만큼만 재생
    public float playLength = 0f;
}

public class SoundManager : MonoBehaviour
{
    [Header("Background Music")]
    public AudioSource backgroundMusicSource;
    public SoundClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioSource effectSource;
    public SoundClip blockPlacedSound;
    public SoundClip[] lineClearSounds;
    public SoundClip gameOverSound;
    public SoundClip gameClearSound;
    public SoundClip reviveSound;

    private int lastClearClipIndex = -1;
    private List<AudioSource> effectSources = new List<AudioSource>();
    public TextMeshProUGUI bgmButtonLabel;
    public TextMeshProUGUI fxButtonLabel;
    public AudioMixer audioMixer; // 인스펙터에서 AudioMixer 할당
    public AudioMixerGroup fxMixerGroup; // 인스펙터에 FX 그룹 할당

    private bool bgmMuted = false;
    private bool fxMuted = false;

    // Helper function to set BGM mute state
    private void SetBGMState(bool muted)
    {
        if (muted)
        {
            audioMixer.SetFloat("BGMVol", -80f);
            if (bgmButtonLabel != null)
            {
                bgmButtonLabel.text = "off";
            }
        }
        else
        {
            audioMixer.SetFloat("BGMVol", 0f);
            if (bgmButtonLabel != null)
            {
                bgmButtonLabel.text = "on";
            }
        }
        bgmMuted = muted;
        PlayerPrefs.SetInt("BGMMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Helper function to set FX mute state
    private void SetFXState(bool muted)
    {
        if (muted)
        {
            audioMixer.SetFloat("FXVol", -80f);
            if (fxButtonLabel != null)
            {
                fxButtonLabel.text = "off";
            }
        }
        else
        {
            audioMixer.SetFloat("FXVol", 0f);
            if (fxButtonLabel != null)
            {
                fxButtonLabel.text = "on";
            }
        }
        fxMuted = muted;
        PlayerPrefs.SetInt("FXMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    void Start()
    {
        if (backgroundMusicSource != null && backgroundMusic.clip != null)
        {
            backgroundMusicSource.clip = backgroundMusic.clip;
            backgroundMusicSource.volume = backgroundMusic.volume;
            backgroundMusicSource.loop = false; // AudioLooper에서 gapless loop 처리하는 경우 loop=false로 설정
            backgroundMusicSource.Play();
            Debug.Log("배경음악 재생 시작");
            lastClearClipIndex = -1;

            if (backgroundMusic.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(backgroundMusicSource, backgroundMusic.playLength));
            }
        }

        if (effectSource != null)
        {
            effectSources.Add(effectSource);
        }
        
        // Load mute settings from PlayerPrefs and apply them
        bgmMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;
        fxMuted = PlayerPrefs.GetInt("FXMuted", 0) == 1;

        SetBGMState(bgmMuted);
        SetFXState(fxMuted);
    }

    public void ResetGame()
    {
        Debug.Log("ResetGame 호출, 배경음악 초기화");
        if (backgroundMusicSource != null && backgroundMusic.clip != null)
        {
            backgroundMusicSource.Stop();
            backgroundMusicSource.clip = backgroundMusic.clip;
            backgroundMusicSource.volume = backgroundMusic.volume;
            backgroundMusicSource.Play();

            if (backgroundMusic.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(backgroundMusicSource, backgroundMusic.playLength));
            }
        }

        lastClearClipIndex = -1;
    }

    public void PlayBGM()
    {
        if (backgroundMusicSource != null && backgroundMusic.clip != null)
        {
            backgroundMusicSource.Stop();
            backgroundMusicSource.clip = backgroundMusic.clip;
            backgroundMusicSource.volume = backgroundMusic.volume;
            backgroundMusicSource.Play();

            if (backgroundMusic.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(backgroundMusicSource, backgroundMusic.playLength));
            }
        }
    }

    public void ResetClearClipIndex()
    {
        lastClearClipIndex = -1;
    }

    private AudioSource GetAvailableEffectSource()
    {
        foreach (var src in effectSources)
        {
            if (!src.isPlaying)
            {
                // 재사용 전 기본값으로 초기화
                src.volume = 1f;
                src.pitch = 1f;
                return src;
            }
        }
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        // FX용 AudioSource에 AudioMixer 그룹 할당
        if(fxMixerGroup != null)
        {
            newSource.outputAudioMixerGroup = fxMixerGroup;
        }
        effectSources.Add(newSource);
        return newSource;
    }

    public void PlayBlockPlacedSound()
    {
        if (blockPlacedSound.clip != null)
        {
            AudioSource src = GetAvailableEffectSource();
            src.clip = blockPlacedSound.clip;
            src.volume = blockPlacedSound.volume;
            src.Play();
            if (blockPlacedSound.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(src, blockPlacedSound.playLength));
            }
        }
    }

    public void PlayReviveSound()
    {
        if (reviveSound.clip != null)
        {
            AudioSource src = GetAvailableEffectSource();
            src.clip = reviveSound.clip;
            src.volume = reviveSound.volume;
            src.Play();
            if (reviveSound.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(src, reviveSound.playLength));
            }
        }
    }

    public void PlayLineClearSound()
    {
        if (lineClearSounds.Length != 0)
        {
            SoundClip clipSetting = lineClearSounds[(lastClearClipIndex + 1) % lineClearSounds.Length];

            if (clipSetting.clip != null)
            {
                AudioSource src = GetAvailableEffectSource();
                src.clip = clipSetting.clip;
                src.volume = clipSetting.volume;
                src.Play();
                lastClearClipIndex++;
                Debug.Log(lastClearClipIndex + "번째 클리어 사운드 재생");
                if (clipSetting.playLength > 0f)
                {
                    StartCoroutine(StopAudioAfterTime(src, clipSetting.playLength));
                }
            }
        }
    }
    

    public void PlayGameOverSound()
    {
        if (gameOverSound.clip != null)
        {
            AudioSource src = GetAvailableEffectSource();
            src.clip = gameOverSound.clip;
            src.volume = gameOverSound.volume;
            src.Play();
            if (gameOverSound.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(src, gameOverSound.playLength));
            }
        }
    }

    public void PlayGameClearSound()
    {
        if (gameClearSound.clip != null)
        {
            AudioSource src = GetAvailableEffectSource();
            src.clip = gameClearSound.clip;
            src.volume = gameClearSound.volume;
            src.Play();
            if (gameClearSound.playLength > 0f)
            {
                StartCoroutine(StopAudioAfterTime(src, gameClearSound.playLength));
            }
        }
    }

    public IEnumerator FadeOutBGMThenPlayGameOver()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            float originalVolume = backgroundMusic.volume;
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                backgroundMusicSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / 2f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            backgroundMusicSource.volume = 0f;
            backgroundMusicSource.Stop();
            // 원래 inspector에 설정한 볼륨으로 복원
            backgroundMusicSource.volume = originalVolume;
        }
    }

    public void ToggleBGM()
    {
        SetBGMState(!bgmMuted);
        Debug.Log("BGM " + (bgmMuted ? "muted" : "unmuted"));
    }

    public void ToggleFX()
    {
        SetFXState(!fxMuted);
        Debug.Log("FX " + (fxMuted ? "muted" : "unmuted"));
    }

    IEnumerator StopAudioAfterTime(AudioSource src, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (src.isPlaying)
        {
            src.Stop();
        }
    }
}