using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    // インスペクターで「どの武器をあげるか」を指定できるようにする
    public int rewardWeaponID = 0;

    // ボスを倒したときや、宝箱を開けたときに呼ぶ関数
    public void GiveWeapon()
    {
        // 指定されたIDの武器を保存
        PlayerPrefs.SetInt("Weapon_" + rewardWeaponID, 1);
        PlayerPrefs.Save();

        Debug.Log("報酬：武器ID " + rewardWeaponID + " を獲得しました");
    }
}