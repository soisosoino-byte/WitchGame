using UnityEngine;

public class OneTimeImage : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("この画像だけのユニークな名前をつけてください（例: Stage1_Tutorial）")]
    public string imageID = "Scene1_Image"; // ★重要: シーンや画像ごとに違う名前にする

    // このスクリプトがついているオブジェクト（画像）そのもの
    private GameObject myObject;

    void Start()
    {
        myObject = this.gameObject;

        // 1. 過去に表示したことがあるかチェック
        // 1なら「見た済み」、0なら「まだ」
        int hasSeen = PlayerPrefs.GetInt(imageID, 0);

        if (hasSeen == 1)
        {
            // すでに見たことがあるなら、即座に消す（非表示にする）
            myObject.SetActive(false);
        }
        else
        {
            // まだ見ていないなら、表示する
            myObject.SetActive(true);
        }
    }

    // 画像がクリックされたら呼ばれる関数
    public void CloseImage()
    {
        // 2. 「見た」という情報を保存する
        PlayerPrefs.SetInt(imageID, 1);
        PlayerPrefs.Save();

        // 3. 画像を消す
        myObject.SetActive(false);

        Debug.Log("画像を閉じました。次は表示されません。");
    }

    // テスト用：また表示されるようにリセットする機能
    [ContextMenu("未読状態に戻す（リセット）")]
    public void ResetStatus()
    {
        PlayerPrefs.DeleteKey(imageID);
        Debug.Log(imageID + " の既読状態をリセットしました");
    }
}
