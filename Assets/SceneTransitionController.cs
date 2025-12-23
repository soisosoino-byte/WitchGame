using System.Collections;
using UnityEngine;
using UnityEngine.UI;         // RawImageを扱うために必要
using UnityEngine.Video;      // VideoPlayerを扱うために必要
using UnityEngine.SceneManagement; // シーン遷移のために必要

public class SceneTransitionController : MonoBehaviour
{
    // ★Inspectorで設定するUI要素への参照★
    public RawImage videoRawImage; // 映像を表示するRawImageコンポーネント
    public VideoPlayer videoPlayer; // 映像を再生するVideoPlayerコンポーネント

    // ★BGM設定★ // ★追加★
    [Header("Audio Settings")]
    public AudioSource bgmAudioSource; // BGMを再生するためのAudioSource
    public AudioClip backgroundMusicClip; // ループ再生するBGMのオーディオクリップ

    // ★効果音設定★ // ★追加★
    public AudioSource sfxAudioSource; // 効果音を再生するためのAudioSource
    public AudioClip shiftPressSFX; // Shiftキーが押された時に再生する効果音のオーディオクリップ


    // ★フェードイン設定★
    public float fadeInDuration = 0.5f; // 映像が透明から表示されるまでの時間



    // ★追加：フェードアウト設定★
    public float fadeOutDuration = 4.0f; // 映像が透明になるまでの時間

    // ★シーン遷移設定★
    public string nextSceneName = "YourNextSceneName"; // Spaceキーで遷移する次のシーンの名前

    private bool isTransitioning = false; // シーン遷移中かどうかのフラグ

    void Awake()
    {
        if (videoRawImage == null)
        {
            videoRawImage = GetComponent<RawImage>();
            if (videoRawImage == null)
            {
                Debug.LogError("VideoRawImageが設定されていません。RawImageコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false;
                return;
            }
        }
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayerが設定されていません。VideoPlayerコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false;
                return;
            }
        }

        // ★BGMと効果音のAudioSourceが設定されているか確認★ // ★追加★
        if (bgmAudioSource == null)
        {
            Debug.LogWarning("BGM AudioSourceが設定されていません。BGMを再生できません。", this);
        }
        if (sfxAudioSource == null)
        {
            Debug.LogWarning("SFX AudioSourceが設定されていません。効果音を再生できません。", this);
        }

        // 初期状態：完全に透明
        Color initialColor = videoRawImage.color;
        initialColor.a = 0f;
        videoRawImage.color = initialColor;
    }

    void Start()
    {
        videoPlayer.Prepare();
        videoPlayer.loopPointReached += OnVideoLoopPointReached;

        StartCoroutine(StartVideoAndFadeIn());

        // ★BGMの再生を開始★ // ★追加★
        if (bgmAudioSource != null && backgroundMusicClip != null)
        {
            bgmAudioSource.clip = backgroundMusicClip;
            bgmAudioSource.loop = true; // ループ再生を有効にする
            bgmAudioSource.Play();
            Debug.Log("BGMを再生開始しました。");
        }
        else if (bgmAudioSource != null && backgroundMusicClip == null)
        {
            Debug.LogWarning("Background Music Clipが設定されていません。BGMを再生できません。", this);
        }
    }

    void Update()
    {
        // Spaceキーが押され、かつシーン遷移中でない場合
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            Debug.Log("Spaceキーが押されました。映像をフェードアウトし、次のシーンへ移動します。");
            StartCoroutine(FadeOutVideoAndMoveToNextScene());
        }

        // ★Shiftキーが押された時に効果音を再生★ // ★追加★
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (sfxAudioSource != null && shiftPressSFX != null)
            {
                sfxAudioSource.PlayOneShot(shiftPressSFX); // OneShotで重ねて再生可能
                Debug.Log("Shiftキーが押されました。効果音を再生します。");
            }
            else if (sfxAudioSource != null && shiftPressSFX == null)
            {
                Debug.LogWarning("Shift Press SFXが設定されていません。効果音を再生できません。", this);
            }
        }
    }

    IEnumerator StartVideoAndFadeIn()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("VideoPlayerの準備が完了しました。再生を開始し、フェードインします。");
        videoPlayer.Play();

        float timer = 0f;
        Color startColor = videoRawImage.color;
        Color endColor = startColor;
        endColor.a = 1f;

        while (timer < fadeInDuration)
        {
            float t = timer / fadeInDuration;
            videoRawImage.color = Color.Lerp(startColor, endColor, t);
            timer += Time.deltaTime;
            yield return null;
        }
        videoRawImage.color = endColor;

        Debug.Log("映像のフェードインが完了しました。");
    }

    // ★追加：映像をフェードアウトさせ、その後にシーン遷移を行うコルーチン★
    IEnumerator FadeOutVideoAndMoveToNextScene()
    {
        isTransitioning = true; // 遷移中フラグを立てる

        // ★BGMをフェードアウトさせるか停止させるか選択★ // ★追加★
        // シーン遷移時にBGMを止めたい場合はここでStop()またはフェードアウト処理を入れる
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop(); // 例: シーン遷移時にBGMを即座に停止
            // あるいは、BGMもフェードアウトさせたい場合
            // StartCoroutine(FadeOutAudio(bgmAudioSource, fadeOutDuration));
        }

        float timer = 0f;
        Color startColor = videoRawImage.color; // 現在のカラー (不透明)
        Color endColor = startColor;
        endColor.a = 0f; // 目標は完全に透明

        // フェードアウトアニメーション
        while (timer < fadeOutDuration)
        {
            float t = timer / fadeOutDuration;
            videoRawImage.color = Color.Lerp(startColor, endColor, t);
            timer += Time.deltaTime;
            yield return null;
        }
        videoRawImage.color = endColor; // 確実に透明に設定

        Debug.Log("映像のフェードアウトが完了しました。次のシーンへ移動します: " + nextSceneName);
        MoveToNextScene(); // フェードアウト完了後にシーン遷移
    }

    // ★BGMフェードアウトの例（必要であればコメントを外して使用）★ // ★追加★
    // IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    // {
    //     float startVolume = audioSource.volume;
    //     float timer = 0f;
    //     while (timer < duration)
    //     {
    //         audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     audioSource.volume = 0f;
    //     audioSource.Stop();
    // }


    void OnVideoLoopPointReached(VideoPlayer vp)
    {
        Debug.Log("映像がループしました。");
    }

    void MoveToNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("次のシーン名が設定されていません！Inspectorで'Next Scene Name'を設定してください。");
        }
    }

    void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
        }
        // シーンが終了する際にBGMを停止しておくと良い
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
    }
}