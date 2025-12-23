using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoMusicPlayer : MonoBehaviour
{
    [Header("動画設定")]
    public VideoPlayer videoPlayer;

    [Header("音の設定")]
    public AudioSource bgmSource; // BGM用スピーカー（SEもここから鳴らします）
    public AudioClip videoBGM;    // 動画中のBGM

    [Header("効果音 (SE) 設定")]
    [Tooltip("映像が始まってすぐに流れる音")]
    public AudioClip startSE;     // ★1つ目の音

    [Tooltip("映像が始まってから数秒後に流れる音")]
    public AudioClip delayedSE;   // ★2つ目の音（追加）

    [Tooltip("2つ目の音が流れるまでの待機時間（秒）")]
    public float seDelay = 2.5f;  // ★2つ目の待機時間

    void Start()
    {
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        StartCoroutine(SyncBGMToVideo(vp));
    }

    IEnumerator SyncBGMToVideo(VideoPlayer vp)
    {
        // 映像が出るまで待つ（ズレ防止）
        yield return new WaitUntil(() => vp.isPlaying && vp.frame > 0);

        // --- 映像スタート！ ---

        // 1. 他のBGMを止める
        StopAllOtherBGM();

        // 2. BGM再生開始
        if (videoBGM != null)
        {
            bgmSource.clip = videoBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 3. 【1つ目】開始SEを鳴らす（即座に）
        if (startSE != null)
        {
            bgmSource.PlayOneShot(startSE);
        }

        // 4. 【2つ目】指定時間待ってから鳴らす
        if (delayedSE != null && seDelay > 0)
        {
            yield return new WaitForSeconds(seDelay); // 待機
            bgmSource.PlayOneShot(delayedSE);         // 再生
        }
    }

    void StopAllOtherBGM()
    {
        GameObject[] bgmObjects = GameObject.FindGameObjectsWithTag("BGM");

        foreach (GameObject obj in bgmObjects)
        {
            if (obj == this.gameObject) continue;

            AudioSource source = obj.GetComponent<AudioSource>();
            if (source != null)
            {
                source.Stop();
            }
        }
    }
}