using UnityEngine;

public class LeafSpawner : MonoBehaviour
{
    [Header("設定")]
    public GameObject leafPrefab;   // 葉っぱのプレハブ
    public float spawnInterval = 0.5f; // 何秒ごとに葉っぱを出すか

    [Header("発生範囲")]
    public float spawnY = 6.0f;     // 発生させる高さ（画面の上外）
    public float xRangeMin = -8.0f; // X座標の左端
    public float xRangeMax = 8.0f;  // X座標の右端

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // 時間が来たら生成
        if (timer > spawnInterval)
        {
            SpawnLeaf();
            timer = 0f;
        }
    }

    void SpawnLeaf()
    {
        // ランダムなX座標を決める
        float randomX = Random.Range(xRangeMin, xRangeMax);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        // 葉っぱを生成する
        Instantiate(leafPrefab, spawnPos, Quaternion.identity);
    }
}