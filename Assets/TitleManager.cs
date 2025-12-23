using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // 前回のバトル開始ボタン用（もし残しておくなら）
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("BattleScene");
    }

    // 【追加】ガチャ画面へ移動するボタンが押されたとき
    public void OnGachaButtonClicked()
    {
        // "GachaScene" の部分
        SceneManager.LoadScene("GachaScene");
    }

    // 【追加】ボスステージへ移動するボタンが押されたとき
    public void OnBossButtonClicked()
    {
        // "BossScene" の部分は、
        SceneManager.LoadScene("sousa");
    }

    public void OnButt2onClicked()
    {
       
        SceneManager.LoadScene("henkou");
    }
}
