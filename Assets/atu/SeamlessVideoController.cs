using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI; // RawImage制御用
using UnityEngine.SceneManagement;

public class SeamlessVideoController : MonoBehaviour
{
    [Header("Video Players (Double Buffer)")]
    [SerializeField] private VideoPlayer playerA;
    [SerializeField] private VideoPlayer playerB;
    [SerializeField] private RawImage imageA;
    [SerializeField] private RawImage imageB;

    [Header("Video URLs")]
    [SerializeField] private string videoUrl1; // Loop
    [SerializeField] private string videoUrl2; // Once
    [SerializeField] private string videoUrl3; // Once
    [SerializeField] private string videoUrl4; // Loop

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgm1;
    [SerializeField] private AudioClip bgm2;

    [Header("Scene")]
    [SerializeField] private string nextSceneName;

    void Start()
    {
        // 初期設定：URLモードにする
        playerA.source = VideoSource.Url;
        playerB.source = VideoSource.Url;

        // 初期状態：Aを表示、Bを隠す
        imageA.enabled = true;
        imageB.enabled = false;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // ==========================================
        // フェーズ 1: 動画1(Loop)再生 + 裏で動画2準備
        // ==========================================

        // BGM1再生
        audioSource.clip = bgm1;
        audioSource.loop = true;
        audioSource.Play();

        // 動画1をPlayerAで再生
        yield return StartCoroutine(PrepareAndPlay(playerA, videoUrl1, true));

        // ★重要：再生中に裏でPlayerBに動画2を準備させておく
        StartCoroutine(PrepareOnly(playerB, videoUrl2));

        // Spaceキー待ち
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // ==========================================
        // フェーズ 2: 動画2再生 + 裏で動画3準備
        // ==========================================

        // BGM停止
        audioSource.Stop();

        // PlayerB(動画2)に切り替え（準備済みなので即再生される）
        SwitchTo(playerB, imageB, playerA, imageA, false); // Loop=false

        // ★裏でPlayerAに動画3を準備させておく
        StartCoroutine(PrepareOnly(playerA, videoUrl3));

        // 動画2の終了待ち（PlayerBの長さ分）
        // ※URL再生時はframe countが不安定な場合があるため、lengthを使用
        yield return new WaitForSeconds((float)playerB.length);

        // ==========================================
        // フェーズ 3: 動画3再生 + 裏で動画4準備
        // ==========================================

        // PlayerA(動画3)に切り替え
        SwitchTo(playerA, imageA, playerB, imageB, false); // Loop=false

        // ★裏でPlayerBに動画4を準備させておく
        StartCoroutine(PrepareOnly(playerB, videoUrl4));

        // 動画3の終了待ち
        yield return new WaitForSeconds((float)playerA.length);

        // ==========================================
        // フェーズ 4: 動画4(Loop)再生 + BGM2再生
        // ==========================================

        // BGM2再生
        audioSource.clip = bgm2;
        audioSource.loop = true;
        audioSource.Play();

        // PlayerB(動画4)に切り替え
        SwitchTo(playerB, imageB, playerA, imageA, true); // Loop=true

        // Spaceキー待ち
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // シーン遷移
        Debug.Log("シーン遷移: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    // プレイヤーと表示を瞬時に切り替えるメソッド
    private void SwitchTo(VideoPlayer nextPlayer, RawImage nextImage, VideoPlayer oldPlayer, RawImage oldImage, bool loop)
    {
        nextPlayer.isLooping = loop;
        nextPlayer.Play(); // 準備済みなので即座に再生される

        nextImage.enabled = true; // 次の画面を表示
        oldImage.enabled = false; // 前の画面を隠す

        oldPlayer.Stop(); // 前のビデオは停止
    }

    // 準備して再生する（初回用）
    private IEnumerator PrepareAndPlay(VideoPlayer player, string url, bool loop)
    {
        player.url = url;
        player.isLooping = loop;
        player.Prepare();
        yield return new WaitUntil(() => player.isPrepared);
        player.Play();
    }

    // 裏で準備だけしておくコルーチン
    private IEnumerator PrepareOnly(VideoPlayer player, string url)
    {
        player.url = url;
        player.Prepare();
        yield return new WaitUntil(() => player.isPrepared);
        // ここではPlayしない。準備完了状態で待機。
    }
}