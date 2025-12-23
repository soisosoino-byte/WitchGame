using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplayManager : MonoBehaviour
{
    [Header("設定")]
    public GameObject[] weaponButtons; // 武器ボタン6個
    public RectTransform equippedMark; // 「装備中」の画像(Image)
    public Vector3 markOffset = new Vector3(0, -50, 0); // ボタンからどれくらい下にずらすか

    void Start()
    {
        UpdateWeaponDisplay();

        // 最初（シーン開始時）に、現在装備中のIDを読み込んでマークを移動させる
        int currentEquipID = PlayerPrefs.GetInt("EquippedWeaponID", 0);
        MoveMarkTo(currentEquipID);
    }

    void UpdateWeaponDisplay()
    {
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            // 持っているかどうかチェック (持っていれば表示)
            int hasWeapon = PlayerPrefs.GetInt("Weapon_" + i, 0);
            weaponButtons[i].SetActive(hasWeapon == 1);
        }
    }

    // ★ボタンが押されたら呼ばれる関数（武器IDを引数で受け取る）
    public void OnWeaponSelect(int weaponID)
    {
        // 1. 装備IDを保存する
        PlayerPrefs.SetInt("EquippedWeaponID", weaponID);
        PlayerPrefs.Save();

        // 2. マークを移動させる
        MoveMarkTo(weaponID);

        Debug.Log("武器ID: " + weaponID + " を装備しました");
    }

    // マークを指定したIDのボタンの場所に移動させる関数
    void MoveMarkTo(int id)
    {
        if (id >= 0 && id < weaponButtons.Length)
        {
            // マークの位置 ＝ ボタンの位置 ＋ ずらし幅(Offset)
            equippedMark.position = weaponButtons[id].transform.position + markOffset;
        }
    }

    // （既存のテスト用コードなどはそのままでOK）
    [ContextMenu("データをリセットする")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        UpdateWeaponDisplay();
        Debug.Log("データをリセットしました");
    }
}