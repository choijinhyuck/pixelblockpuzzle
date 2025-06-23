using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioLooper : MonoBehaviour
{
    [SerializeField] private double loopStartTime; // 반복 시작 시점 (초)
    [SerializeField] private double loopEndTime;   // 반복 종료 시점 (초)
    [SerializeField] private AudioSource audioSource;

    private int loopStartSamples;
    private int loopEndSamples;
    private int loopLengthSamples;

    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        loopStartSamples = (int)(loopStartTime * audioSource.clip.frequency);
        loopEndSamples = (int)(loopEndTime * audioSource.clip.frequency);
        loopLengthSamples = loopEndSamples - loopStartSamples;

        // 오디오가 반복 종료 샘플에 도달하면 반복 시작 샘플로 되돌림
        if (audioSource.timeSamples >= loopEndSamples)
        {
            audioSource.timeSamples -= loopLengthSamples;
        }
    }
}