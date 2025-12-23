using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("ダッシュ設定")]
    [Tooltip("ダッシュの速さ")]
    public float dashSpeed = 20f;
    [Tooltip("ダッシュしている時間（秒）")]
    public float dashDuration = 0.2f;
    [Tooltip("次にダッシュできるまでの待ち時間（秒）")]
    public float cooldownTime = 1.0f;

    // ★変更: アニメーション名ではなく、アニメーターのパラメータ名を指定
    [Tooltip("Animatorで作成したBoolパラメータ名")]
    public string dashParamName = "IsDashing";

    [Header("状態（確認用）")]
    public bool isDashing = false;
    public bool canDash = true;

    private Rigidbody2D rb;
    private Animator anim;
    private control playerControl;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerControl = GetComponent<control>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.C)) && canDash)
        {
            StartCoroutine(DashAction());
        }
    }

    IEnumerator DashAction()
    {
        canDash = false;
        isDashing = true;

        if (playerControl != null)
        {
            playerControl.enabled = false;
            playerControl.StopMoveSound();
        }

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        // 向きの判定
        float direction = 1f;
        if (transform.localScale.x < 0)
        {
            direction = -1f;
        }
        else if (spriteRenderer != null && spriteRenderer.flipX == true)
        {
            direction = -1f;
        }

        // 速度を与える
        rb.velocity = new Vector2(direction * dashSpeed, 0);

        // --- ★ここを修正: アニメーション開始 ---
        // 直接Playするのではなく、パラメータをONにして遷移させる
        if (anim != null)
        {
            anim.SetBool(dashParamName, true);
        }
        // ------------------------------------

        // ダッシュ時間分待つ
        yield return new WaitForSeconds(dashDuration);

        // --- ★ここを追加: アニメーション終了 ---
        // パラメータをOFFにして元の状態に戻す
        if (anim != null)
        {
            anim.SetBool(dashParamName, false);
        }
        // ------------------------------------

        rb.gravityScale = originalGravity;
        rb.velocity = Vector2.zero;
        isDashing = false;

        if (playerControl != null)
        {
            playerControl.enabled = true;
        }

        yield return new WaitForSeconds(cooldownTime);

        canDash = true;
    }
}