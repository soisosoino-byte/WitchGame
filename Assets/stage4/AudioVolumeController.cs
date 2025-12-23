using UnityEngine;

public class AudioVolumeController : MonoBehaviour
{
    [Header("オーディオソース")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    // [Range(最小, 最大)] をつけると、Inspectorがスライダーになります
    [Header("音量設定 (0.0 ～ 1.0)")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float seVolume = 0.5f;

    // Inspectorの値が変更された時に自動で呼ばれる機能
    private void OnValidate()
    {
        UpdateVolumes();
    }

    // ゲーム開始時にも適用
    private void Start()
    {
        UpdateVolumes();
    }

    // 音量をAudioSourceに反映させる処理
    private void UpdateVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        if (seSource != null)
        {
            seSource.volume = seVolume;
        }
    }
}