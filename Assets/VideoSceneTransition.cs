using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要
using System.Collections;

public class VideoSceneTransition : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("移動先のシーン名")]
    public string nextSceneName = "GameScene"; // ここに移動先のシーン名を入れる

    [Tooltip("自動で遷移するまでの時間（秒）")]
    public float autoSkipTime = 4.0f;

    private bool isSkipped = false; // 二重遷移防止用フラグ

    void Start()
    {
        // 自動遷移のカウントダウンを開始
        StartCoroutine(AutoSkipSequence());
    }

    void Update()
    {
        // まだ遷移処理が走っておらず、Spaceキーが押されたら
        if (!isSkipped && Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextScene();
        }
    }

    // 指定時間待ってから遷移するコルーチン
    IEnumerator AutoSkipSequence()
    {
        // 4秒待つ
        yield return new WaitForSeconds(autoSkipTime);

        // まだSpaceキーで遷移していなければ、自動で遷移
        if (!isSkipped)
        {
            GoToNextScene();
        }
    }

    // シーン遷移処理
    void GoToNextScene()
    {
        isSkipped = true; // フラグを立てて重複処理を防ぐ
        Debug.Log("次のシーンへ移動します");
        SceneManager.LoadScene(nextSceneName);
    }
}
