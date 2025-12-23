using UnityEngine;

public class FallingLeaf : MonoBehaviour
{
    [Header("動きの設定")]
    public float fallSpeed = 2.0f;       // 落ちる基本速度
    public float rotationSpeed = 100.0f; // 回転する速度
    public float swayAmount = 0.5f;      // 左右に揺れる幅
    public float swaySpeed = 2.0f;       // 左右に揺れる速さ

    [Header("消滅設定")]
    public float destroyY = -6.0f;       // このY座標より下に行ったら消える

    private float startX; // 生成されたときのX座標
    private float timeOffset; // 揺れのタイミングをずらすための乱数

    void Start()
    {
        startX = transform.position.x;
        // 葉っぱごとに揺れのタイミングをランダムにずらす
        timeOffset = Random.Range(0f, 10f);

        // 落ちる速度や回転速度にも少しランダム性を持たせる（自然に見えるコツ）
        fallSpeed += Random.Range(-0.5f, 0.5f);
        rotationSpeed += Random.Range(-30f, 30f);
    }

    void Update()
    {
        // 1. 下に落ちる動き
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // 2. くるくる回転する動き (Z軸回転)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 3. 左右にゆらゆらする動き (Sin波を使用)
        // 「元のX座標」+ 「揺れ幅 * Sin(時間 * 速さ + ズレ)」
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * swaySpeed) * swayAmount;

        // 座標を更新（Yは今の位置、Xは計算した位置）
        Vector3 pos = transform.position;
        pos.x = newX;
        transform.position = pos;

        // 4. 画面の下の方に行ったら削除する（メモリ節約）
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
