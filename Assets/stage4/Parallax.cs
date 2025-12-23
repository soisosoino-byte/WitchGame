using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("カメラの設定")]
    public GameObject cam; // カメラをアサインする変数

    [Header("視差の強さ (0?1)")]
    // 1に近いほどカメラと同じ動き（遠くに見える）、0だと動かない
    public float parallaxEffect;

    private float startPos; // 初期のX位置

    void Start()
    {
        // 最初の位置を記憶
        startPos = transform.position.x;
    }

    void Update()
    {
        // カメラの現在位置に基づいて、背景がどれくらい動くべきか計算
        // カメラが動いた距離 * 係数
        float dist = (cam.transform.position.x * parallaxEffect);

        // 背景の位置を更新 (Y軸とZ軸はそのまま)
        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);
    }
}