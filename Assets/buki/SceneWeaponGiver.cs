using UnityEngine;

public class SceneWeaponGiver : MonoBehaviour
{
    [Header("入手させる武器の設定")]
    [Tooltip("入手させたい武器のIDを入力してください (0～5)")]
    public int targetWeaponID = 0; // ここに入力した番号の武器が手に入ります

    void Start()
    {
        GiveWeapon();
    }

    void GiveWeapon()
    {
        // データの名前（キー）を作る
        string key = "Weapon_" + targetWeaponID;

        // すでに持っているかチェックする
        int hasWeapon = PlayerPrefs.GetInt(key, 0);

        if (hasWeapon == 1)
        {
            // すでに持っている場合は何もしない（ログだけ出す）
            Debug.Log("武器ID: " + targetWeaponID + " は既に入手済みです。");
        }
        else
        {
            // 持っていない場合のみ、入手処理をする
            PlayerPrefs.SetInt(key, 1); // 1 = 持っている
            PlayerPrefs.Save(); // 保存

            Debug.Log("おめでとうございます！武器ID: " + targetWeaponID + " を新しく入手しました！");

            // もし「入手しました！」という文字を画面に出したい場合は
            // ここにUIを表示する処理などを追加できます
        }
    }
}