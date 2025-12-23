using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tatie : MonoBehaviour
{
    // ★変更点: string を string[] (配列) に変えます
    // インスペクターで「Size」を6にして、各武器ごとのアニメ名を入力してください
    public string[] standAnimes;
    public string[] walkAnimes;
    public string[] upAnimes;
    public string[] downAnimes;
    public string[] shagamiAnimes;
    public string[] kougekiAnimes;
    public string[] kuchukougekiAnimes;

    string nowMode = "";
    bool shagamiFlag = false;
    bool attackFlag = false;
    int equippedID = 0; // 現在装備している武器ID

    Rigidbody2D rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();

        // ★保存された「装備中の武器ID」を読み込む (デフォルトは0)
        equippedID = PlayerPrefs.GetInt("EquippedWeaponID", 0);

        // 最初のアニメーションをセット（範囲外エラー防止付き）
        if (standAnimes.Length > equippedID)
        {
            nowMode = standAnimes[equippedID];
        }
    }

    void Update()
    {
        if (shagamiFlag || attackFlag) return;

        // ★配列から[equippedID]番目のアニメ名を取り出すように変更
        if (rbody.velocity.y > 0)
        {
            nowMode = upAnimes[equippedID];
        }
        else if (rbody.velocity.y < 0)
        {
            nowMode = downAnimes[equippedID];
        }
        else if (Input.GetKey("right") || Input.GetKey("left"))
        {
            nowMode = walkAnimes[equippedID];
        }
        else
        {
            nowMode = standAnimes[equippedID];
        }

        if (!IsGrounded() && Input.GetKey("Shift") && !shagamiFlag)
        {
            shagamiFlag = true;
            nowMode = shagamiAnimes[equippedID];
            Invoke("ResetShagamiAnime", 0.4f);
        }

        if (Input.GetKeyDown(KeyCode.V) && !attackFlag)
        {
            attackFlag = true;
            // 地上か空中かで分岐
            if (IsGrounded())
            {
                nowMode = kougekiAnimes[equippedID];
            }
            else
            {
                nowMode = kuchukougekiAnimes[equippedID];
            }
            Invoke("ResetAttackAnime", 0.3f);
        }
    }

    private void FixedUpdate()
    {
        // アニメーション再生（空文字でないか確認）
        if (!string.IsNullOrEmpty(nowMode))
        {
            this.GetComponent<Animator>().Play(nowMode);
        }
    }

    void ResetShagamiAnime()
    {
        nowMode = standAnimes[equippedID];
        shagamiFlag = false;
    }

    void ResetAttackAnime()
    {
        nowMode = standAnimes[equippedID];
        attackFlag = false;
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
    }
}
