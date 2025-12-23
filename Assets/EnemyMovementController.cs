using UnityEngine;
using System.Collections;

public class EnemyMovementController : MonoBehaviour
{
    private Animator animator;
    private Vector3 currentTargetPosition;

    [Header("Animation Parameters")]
    public string moveAnimationTrigger = "Move";
    public string idleAnimationTrigger = "Idle";
    // ★追加: 攻撃パターンごとのアニメーションを柔軟に扱うための汎用トリガー設定★
    // 必要に応じてここに他のトリガー名を追加することもできますが、
    // 基本的にはSetTriggerAndMoveToPositionで任意のトリガー名を渡す形が柔軟です。

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("EnemyMovementController: Animatorコンポーネントが見つかりません。", this);
        }
        if (animator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
        {
            animator.SetTrigger(idleAnimationTrigger);
        }
    }

    /// <summary>
    /// 敵を指定された目標座標へ指定された時間で移動させ、移動中は指定のアニメーショントリガーを再生します。
    /// </summary>
    /// <param name="targetPos">移動目標座標</param>
    /// <param name="duration">移動にかかる時間</param>
    /// <param name="animationTrigger">移動中に再生するアニメーションのトリガー名</param>
    public IEnumerator SetTriggerAndMoveToPosition(Vector3 targetPos, float duration, string animationTrigger) // ★変更: アニメーションのトリガー名を引数に追加★
    {
        currentTargetPosition = targetPos;
        Vector3 startPos = transform.position;
        float timer = 0f;

        // 指定されたアニメーションを再生
        if (animator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            animator.SetTrigger(animationTrigger);
            Debug.Log($"EnemyAnimator: Trigger '{animationTrigger}' を設定しました。");
        }

        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPos, currentTargetPosition, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = currentTargetPosition;

        // 移動完了後、待機アニメーションを再生
        if (animator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
        {
            animator.SetTrigger(idleAnimationTrigger);
            Debug.Log($"EnemyAnimator: Trigger '{idleAnimationTrigger}' を設定しました。(移動完了後)");
        }
    }

    /// <summary>
    /// 敵を指定された目標座標へ指定された時間で移動させます。(デフォルトのMoveトリガーを使用)
    /// </summary>
    /// <param name="targetPos">移動目標座標</param>
    /// <param name="duration">移動にかかる時間</param>
    public IEnumerator MoveToPosition(Vector3 targetPos, float duration)
    {
        // デフォルトのmoveAnimationTriggerを使用
        yield return StartCoroutine(SetTriggerAndMoveToPosition(targetPos, duration, moveAnimationTrigger));
    }


    /// <summary>
    /// 敵を待機状態にし、待機アニメーションを再生します。
    /// </summary>
    public void SetIdle()
    {
        if (animator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
        {
            animator.SetTrigger(idleAnimationTrigger);
            Debug.Log($"EnemyAnimator: Trigger '{idleAnimationTrigger}' を設定しました。(SetIdle)");
        }
    }
}
