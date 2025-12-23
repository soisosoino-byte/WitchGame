using UnityEngine;
using System.Collections;

public class RotatingBeam : MonoBehaviour
{
    [Header("ビーム設定")]
    public float rotationSpeed = 100f; // 回転する速さ
    public int damage = 1;
    public float duration = 5.0f; // ビームが出ている時間

    // ★追加: フェードアウトにかける時間（ご指定の0.3秒）
    public float fadeDuration = 0.3f;

    void Start()
    {
        // いきなりDestroyせず、寿命管理のコルーチンを開始
        StartCoroutine(BeamLifeCycle());
    }

    void Update()
    {
        // 時計回り（Z軸マイナス方向）に回転
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }

    // ★追加: ビームの寿命とフェードアウトを管理するコルーチン
    IEnumerator BeamLifeCycle()
    {
        // 1. ビームの持続時間分だけ待つ（この間も回転し続ける）
        yield return new WaitForSeconds(duration);

        // 2. フェードアウト処理開始
        // 子要素（Beam1, Beam2）にある全てのSpriteRendererを取得
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        float timer = 0f;
        // 元の色（透明度）を覚えておくための配列
        Color[] startColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            startColors[i] = sprites[i].color;
        }

        // 指定時間（0.3秒）かけて透明にする
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration; // 0から1へ進む

            // 全てのビーム画像の透明度を操作
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                {
                    // 元のアルファ値 -> 0 に滑らかに変化
                    float newAlpha = Mathf.Lerp(startColors[i].a, 0f, progress);

                    Color color = startColors[i];
                    color.a = newAlpha;
                    sprites[i].color = color;
                }
            }
            yield return null;
        }

        // 3. 完全に透明になったら削除
        Destroy(gameObject);
    }

    // 当たり判定
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kyara"))
        {
            var playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
            }
        }
    }
}