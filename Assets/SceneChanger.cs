using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // この関数ひとつで、あらゆるシーン移動に対応します
    // カッコの中の string sceneName が「行き先」を受け取る箱です
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
