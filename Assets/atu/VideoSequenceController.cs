using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class VideoSequenceController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [Header("Video URLs")]
    // ファイルパス（例: C:/Videos/movie.mp4）または Web上のURL（http://...）
    // StreamingAssetsフォルダ内の場合は "file://" + Application.streamingAssetsPath + "/ファイル名.mp4"
    [SerializeField] private string videoUrl1; // ループ
    [SerializeField] private string videoUrl2; // 1回のみ
    [SerializeField] private string videoUrl3; // 1回のみ
    [SerializeField] private string videoUrl4; // ループ

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgm1;
    [SerializeField] private AudioClip bgm2;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName; // 移動先のシーン名

    void Start()
    {
        // VideoPlayerのモードをURLモードに強制設定
        videoPlayer.source = VideoSource.Url;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // --- フェーズ 1: 動画1(ループ) + BGM1(ループ) ---

        audioSource.clip = bgm1;
        audioSource.loop = true;
        audioSource.Play();

        // 動画再生処理（ループ設定）
        yield return StartCoroutine(PlayVideo(videoUrl1, true));

        // Spaceキーが押されるまで待機
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // --- フェーズ 2: BGM停止 -> 動画2 -> 動画3 ---

        audioSource.Stop();

        // 動画2再生 (ループなし)
        yield return StartCoroutine(PlayVideo(videoUrl2, false));

        // 動画2の長さ分待機（URLの場合、再生開始後に長さを取得する必要があるためPlayVideo内で処理しません）
        // 再生が終わるまで待機（isPlayingがfalseになるまで待つとバッファ等で不安定なため、長さで待つのが確実です）
        yield return new WaitForSeconds((float)videoPlayer.length);

        // 動画3再生 (ループなし)
        yield return StartCoroutine(PlayVideo(videoUrl3, false));

        // 動画3の長さ分待機
        yield return new WaitForSeconds((float)videoPlayer.length);

        // --- フェーズ 3: 動画4(ループ) + BGM2(ループ) ---

        audioSource.clip = bgm2;
        audioSource.loop = true;
        audioSource.Play();

        // 動画4再生 (ループ設定)
        yield return StartCoroutine(PlayVideo(videoUrl4, true));

        // Spaceキーが押されるまで待機
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // --- シーン遷移 ---
        Debug.Log("シーン遷移: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    // 動画再生を管理するヘルパーコルーチン
    private IEnumerator PlayVideo(string url, bool isLooping)
    {
        videoPlayer.url = url;
        videoPlayer.isLooping = isLooping;

        // 準備開始
        videoPlayer.Prepare();

        // 準備完了まで待機
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Play();
    }
}