using UnityEngine;
using System.Collections;

public class DelayedAttackObject : MonoBehaviour
{
    // ボスからセットされる変数
    private Sprite targetSprite; // 変化後の画像
    private float totalDelayTime; // 攻撃開始までの総時間
    private int damage = 1;      // ダメージ量

    [Header("出現演出設定")]
    [Tooltip("大きくなって現れるまでにかかる時間（秒）")]
    public float appearDuration = 0.5f; // 例: 0.5秒かけて出現

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isActive = false; // 攻撃判定が有効かどうか
    private Vector3 targetScale;   // 本来の大きさ（プレハブ設定時のスケール）

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // 最初は当たり判定を無効にしておく
        if (col != null)
        {
            col.enabled = false;
        }

        // 本来の大きさを記憶し、最初は見えないようにスケールを0にする
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    // ボス側から呼び出して設定を行う関数
    public void Setup(Sprite newSprite, float delay, int dmg)
    {
        targetSprite = newSprite;
        totalDelayTime = delay;
        damage = dmg;

        // シーケンス開始
        StartCoroutine(MainSequence());
    }

    IEnumerator MainSequence()
    {
        // 1. 出現演出（大きくなる）
        float timer = 0f;
        while (timer < appearDuration)
        {
            timer += Time.deltaTime;
            // 0 から targetScale へ滑らかに変化
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / appearDuration);
            yield return null;
        }
        transform.localScale = targetScale; // 念のため最終値をセット

        // 2. 攻撃開始までの残り時間を待機
        // 「総待ち時間」から「出現に使った時間」を引く
        float remainingWaitTime = totalDelayTime - appearDuration;

        // もし待ち時間が残っているなら待つ
        // (出現時間の方が長い設定の場合は即座に次に進む)
        if (remainingWaitTime > 0)
        {
            yield return new WaitForSeconds(remainingWaitTime);
        }

        // 3. 攻撃有効化（画像変化、判定ON）
        isActive = true;
        if (spriteRenderer != null && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite; // 画像変更
        }
        if (col != null)
        {
            col.enabled = true; // 当たり判定ON
        }
        // ※このタイミングでボス側が効果音を鳴らします

        // 4. 攻撃持続(0.2秒)
        yield return new WaitForSeconds(0.2f);

        // 5. 消滅
        Destroy(gameObject);
    }

    // 当たり判定（変更なし）
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && collision.CompareTag("Kyara"))
        {
            var playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
            }
        }
    }
}