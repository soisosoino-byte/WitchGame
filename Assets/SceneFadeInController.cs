using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Imageを扱うために必要

public class SceneFadeInController : MonoBehaviour
{
    // ★Inspectorで設定するUI Imageへの参照★
    public Image fadePanel; // 画面全体を覆うImageコンポーネント

    // ★フェードイン設定★
    public float fadeInDuration = 1.0f; // 真っ暗な状態からシーンが明るくなるまでの時間

    // ★★★ここからサウンド関連の追加★★★
    [Header("Audio Settings")]
    public AudioClip introSound;    // シーン開始時の短い効果音
    public AudioClip combatBGM;     // その後にループする戦闘BGM

    private AudioSource audioSource; // このGameObjectにアタッチされたAudioSource
    // ★★★ここまでサウンド関連の追加★★★

    void Awake()
    {
        // Imageコンポーネントの参照を取得
        if (fadePanel == null)
        {
            fadePanel = GetComponent<Image>();
            if (fadePanel == null)
            {
                Debug.LogError("FadePanelが設定されていません。Imageコンポーネントがアタッチされているか確認してください。", this);
                this.enabled = false;
                return;
            }
        }

        // 初期状態：完全に不透明な黒
        fadePanel.color = new Color(0f, 0f, 0f, 1f);

        // ★追加：AudioSourceコンポーネントの取得★
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0; // 2Dサウンドに設定
            audioSource.playOnAwake = false; // シーン開始時に自動再生しない
        }
    }

    void Start()
    {
        // フェードインアニメーションを開始
        StartCoroutine(FadeInScene());
    }

    IEnumerator FadeInScene()
    {
        float timer = 0f;
        Color startColor = fadePanel.color; // 現在のカラー (a=1f)
        Color endColor = startColor;
        endColor.a = 0f; // 目標は完全に透明

        Debug.Log("シーンのフェードインを開始します。");

        // フェードインアニメーション
        while (timer < fadeInDuration)
        {
            float t = timer / fadeInDuration;
            fadePanel.color = Color.Lerp(startColor, endColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // 確実に透明に設定し、その後GameObjectを非アクティブにするかDestroyする
        fadePanel.color = endColor;
        gameObject.SetActive(false); // フェードイン完了後、パネルを非表示にする

        Debug.Log("シーンのフェードインが完了しました。");

        // ★★★ここからサウンド再生処理の追加★★★
        if (introSound != null)
        {
            Debug.Log($"イントロサウンド {introSound.name} を再生します。");
            audioSource.PlayOneShot(introSound); // イントロサウンドを単発で再生
            yield return new WaitForSeconds(introSound.length); // イントロサウンドの再生が終わるまで待機
        }
        else
        {
            Debug.LogWarning("Intro Soundが設定されていません。", this);
        }

        if (combatBGM != null)
        {
            Debug.Log($"戦闘BGM {combatBGM.name} を再生します。");
            audioSource.clip = combatBGM; // BGMクリップを設定
            audioSource.loop = true;      // ループ再生を有効に
            audioSource.Play();           // BGMの再生を開始
        }
        else
        {
            Debug.LogWarning("Combat BGMが設定されていません。", this);
        }
        // ★★★ここまでサウンド再生処理の追加★★★
    }
}