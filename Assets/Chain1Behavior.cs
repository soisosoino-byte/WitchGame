using System.Collections;
using UnityEngine;
using System; // Actionのために必要

public class Chain1Behavior : MonoBehaviour
{
    private float targetY;
    private float moveSpeed;
    private bool hasReachedEnd = false; // 目的のY座標に到達したか

    private SpriteRenderer spriteRenderer; // ★追加: SpriteRendererへの参照★

    // ★追加: Chain1の新しい設定変数★
    [Header("Chain1 Appearance & Disappearance")]
    public Vector3 initialScale = new Vector3(1f, 1f, 1f); // 初期スケール
    public float displayDurationAfterArrival = 1.0f; // 目的Y座標到達後、消えるまでの時間
    public float fadeOutDuration = 0.3f; // 消える時の透明化にかかる時間

    // Chain1が完全に消えたことをEnemyAttackManagerに通知するイベント
    public event Action OnChain1Destroyed;

    // EnemyAttackManagerから設定を受け取るためのセットアップメソッド
    public void SetupChain1(float endY, float speed)
    {
        targetY = endY;
        moveSpeed = speed;
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRendererを取得
        if (spriteRenderer == null)
        {
            Debug.LogError("Chain1Behavior: SpriteRendererがアタッチされていません！");
        }

        // ★追加: 初期スケールを適用★
        transform.localScale = initialScale;

        // ★追加: Chain1のライフサイクルを管理するコルーチンを開始★
        StartCoroutine(Chain1Lifecycle());
    }

    private IEnumerator Chain1Lifecycle()
    {
        // 初期状態は完全に不透明にする
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 1f;
            spriteRenderer.color = currentColor;
        }

        // Chain1が目的のY座標に到達するまで移動
        while (!hasReachedEnd)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), moveSpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.y - targetY) < 0.01f) // ほぼ到達したら
            {
                Debug.Log("Chain1が目的のY座標に到達しました。");
                hasReachedEnd = true;
            }
            yield return null;
        }

        // 目的のY座標に到達後、指定された時間表示を維持
        Debug.Log($"Chain1が目的Y座標到達後、{displayDurationAfterArrival}秒間表示されます。");
        yield return new WaitForSeconds(displayDurationAfterArrival);

        // ★追加: 0.3秒かけて透明にするフェードアウト処理★
        if (spriteRenderer != null)
        {
            Debug.Log($"Chain1が{fadeOutDuration}秒かけて透明になります。");
            float timer = 0f;
            Color startColor = spriteRenderer.color; // 現在の色 (A=1)
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 透明な色
            while (timer < fadeOutDuration)
            {
                spriteRenderer.color = Color.Lerp(startColor, endColor, timer / fadeOutDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = endColor; // 確実に透明に設定
        }

        // Chain1オブジェクトを削除する前にイベントを発火
        OnChain1Destroyed?.Invoke();
        Destroy(gameObject);
        Debug.Log("Chain1オブジェクトを削除しました。");
    }

    // EnemyAttackManagerがChain1の完了を待つために使用
    public bool IsAtEndPosition()
    {
        return hasReachedEnd;
    }

    // デバッグ用に、Chain1が止まる位置を可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, targetY, transform.position.z), 0.5f);
    }
}
