using UnityEngine;

public class SceneVisitRecorder : MonoBehaviour
{
    [Header("保存設定")]
    [Tooltip("このシーンに来たことを記録するID（例: Stage1_Visited）。メニュー画面の設定と同じにしてください。")]
    public string visitID = "Stage1_Visited";

    void Start()
    {
        // このシーンが読み込まれた瞬間に「1 (来たことがある)」を保存
        PlayerPrefs.SetInt(visitID, 1);
        PlayerPrefs.Save();

        Debug.Log("訪問記録: " + visitID + " を保存しました。");
    }
}
