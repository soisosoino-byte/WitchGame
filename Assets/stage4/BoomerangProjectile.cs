using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    public int damage = 1;
    [Header("回転設定")]
    public float rotationSpeed = 720f; // 1秒間の回転角度

    private Vector3 p0, p1, p2, p3; // 軌道を決める4つの点
    private float duration; // 飛ぶ時間
    private float timeElapsed = 0f;
    private bool isInitialized = false;

    // 初期化関数（ボスから呼ばれる）
    public void Setup(Vector3 startPos, Vector3 forwardDir, Vector3 sideDir, float distance, float width, float time)
    {
        duration = time;

        // --- 軌道の4点（3次ベジェ曲線）を計算 ---
        // P0: スタート地点（ボスの手元）
        p0 = startPos;

        // P1: 制御点1（横に大きく広がる点）
        //     スタート位置から「横幅(width)」分だけ横にずらし、「距離(distance)」の20%進んだ場所
        p1 = startPos + (sideDir * width) + (forwardDir * (distance * 0.2f));

        // P2: 制御点2（一番遠い折り返し地点付近）
        //     「距離(distance)」まで進み、かつ「横幅(width)」分ずれた場所
        p2 = startPos + (sideDir * width) + (forwardDir * distance);

        // P3: ゴール地点（スタート地点に戻る）
        p3 = startPos;

        isInitialized = true;

        // 寿命設定（万が一戻り損ねた時用）
        Destroy(gameObject, duration + 0.1f);
    }

    void Update()
    {
        if (!isInitialized) return;

        // 時間を経過させる
        timeElapsed += Time.deltaTime;
        float t = timeElapsed / duration; // 0(開始) ～ 1(終了) の値

        if (t >= 1f)
        {
            Destroy(gameObject); // 戻ってきたら消滅
            return;
        }

        // --- 3次ベジェ曲線の計算式 ---
        // (1-t)^3*P0 + 3(1-t)^2*t*P1 + 3(1-t)t^2*P2 + t^3*P3
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = u * u * u;
        float ttt = t * t * t;

        Vector3 p = (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
        transform.position = p;

        // --- 画像を回転させる ---
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kyara"))
        {
            var playerHP = collision.GetComponent<hpdesu2>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
            }
            // ブーメランは貫通することが多いので、ここではDestroyしない
            // （もし当たったら消したい場合は Destroy(gameObject); を入れる）
        }
    }
}