using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("基本設定")]
    public float startWaitTime = 2.0f;
    public float intervalTime = 1.5f;

    [Header("オーディオ設定")] // ★追加
    public AudioClip soundPattern1Shot; // パターン1の発射音
    private AudioSource audioSource;    // 音を鳴らすスピーカー

    [Header("攻撃パターンの確率（重み）")]
    [Range(0, 100)] public int pattern1Weight = 25;
    [Range(0, 100)] public int pattern2Weight = 25;
    [Range(0, 100)] public int pattern3Weight = 25;
    [Range(0, 100)] public int pattern4Weight = 25;

    // --- パターン1設定 ---
    [Header("パターン1の設定")]
    public GameObject attack1Prefab;
    public Vector3 pattern1TargetPos = new Vector3(5, 0, 0);
    public float enemyMoveSpeed = 3.0f;
    public float chargeTime = 2.0f;

    // --- パターン2設定 ---
    [Header("パターン2の設定")]
    public GameObject beamUnitPrefab;
    public Vector3 beamUnitSpawnPos = new Vector3(10, 0, 0);
    public string animPattern2 = "Chant";
    public Vector3 pattern2TargetPos = new Vector3(0, 3, 0);

    // --- パターン3設定 ---
    [Header("パターン3の設定")]
    public GameObject fistPrefab;
    public Vector3 pattern3TargetPos = new Vector3(0, 4, 0);
    public string animPattern3 = "Attack";
    [Header("拳の配置設定")]
    public int fistCount = 6;
    public float fistSpacing = 2.5f;
    public float fistStartX = -6.0f;
    public float fistAppearanceInterval = 0.2f;
    [Header("拳の動き設定")]
    public float fistSpawnY = 10.0f;
    public float fistReadyY = 3.0f;
    public float fistSmashY = -3.5f;
    public float fistMoveSpeed = 20.0f;
    public float smashInterval = 0.8f;
    public int repeatCount = 4;
    public float fistVibrateDuration = 0.4f;
    public float fistVibrateStrength = 0.1f;

    // --- パターン4設定 ---
    [Header("パターン4の設定")]
    public GameObject bombPrefab;
    public Vector3 pattern4EnemyPos = new Vector3(0, 2, 0);
    public string animPattern4 = "Attack";
    [Header("爆弾の動作設定")]
    public List<Vector3> bombTargetPositions = new List<Vector3>();
    public float bombMoveDuration = 1.0f;
    public float bombBlinkDuration = 1.5f;
    public float bombSpawnInterval = 0.5f;

    [Header("アニメーション名設定")]
    public string animMove = "Move";
    public string animCharge = "Charge";
    public string animFire = "Attack";
    public string animIdle = "Idle";

    private int lastPattern = -1;
    private int bannedPattern = -1;
    private int banTurnCount = 0;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // ★追加: AudioSourceを取得（なければ追加）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        yield return new WaitForSeconds(startWaitTime);

        while (true)
        {
            int nextPattern = SelectNextPattern();

            if (nextPattern == 0)
            {
                Debug.LogWarning("攻撃パターン未選択。待機。");
            }
            else
            {
                Debug.Log("★攻撃開始: パターン " + nextPattern);
                switch (nextPattern)
                {
                    case 1: yield return StartCoroutine(ActionPattern1()); break;
                    case 2: yield return StartCoroutine(ActionPattern2()); break;
                    case 3: yield return StartCoroutine(ActionPattern3()); break;
                    case 4: yield return StartCoroutine(ActionPattern4()); break;
                }
            }

            if (animator != null) animator.Play(animIdle);
            Debug.Log("休憩中...");
            yield return new WaitForSeconds(intervalTime);
        }
    }

    int SelectNextPattern()
    {
        if (banTurnCount > 0)
        {
            banTurnCount--;
            if (banTurnCount == 0) bannedPattern = -1;
        }

        Dictionary<int, int> candidates = new Dictionary<int, int>();
        if (bannedPattern != 1) candidates.Add(1, pattern1Weight);
        if (bannedPattern != 2) candidates.Add(2, pattern2Weight);
        if (bannedPattern != 3) candidates.Add(3, pattern3Weight);
        if (bannedPattern != 4) candidates.Add(4, pattern4Weight);

        int totalWeight = 0;
        foreach (var kvp in candidates) totalWeight += kvp.Value;

        if (totalWeight == 0) return 0;

        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;
        int selectedPattern = 0;

        foreach (var kvp in candidates)
        {
            currentSum += kvp.Value;
            if (randomValue < currentSum)
            {
                selectedPattern = kvp.Key;
                break;
            }
        }

        if (selectedPattern == lastPattern)
        {
            bannedPattern = selectedPattern;
            banTurnCount = 2;
        }
        lastPattern = selectedPattern;
        return selectedPattern;
    }

    // =================================================
    // 各行動パターンの実装
    // =================================================

    IEnumerator ActionPattern1()
    {
        Debug.Log("行動1開始");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern1TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern1TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animCharge);
        yield return new WaitForSeconds(chargeTime);
        if (animator != null) animator.Play(animFire);

        if (attack1Prefab != null) Instantiate(attack1Prefab, transform.position, Quaternion.identity);

        // ★追加: 発射音を鳴らす
        if (soundPattern1Shot != null) audioSource.PlayOneShot(soundPattern1Shot);

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ActionPattern2()
    {
        Debug.Log("行動2開始");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern2TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern2TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animPattern2);
        if (beamUnitPrefab != null) Instantiate(beamUnitPrefab, beamUnitSpawnPos, Quaternion.identity);
        yield return new WaitForSeconds(7.0f);
    }

    IEnumerator ActionPattern3()
    {
        Debug.Log("行動3: 拳攻撃");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern3TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern3TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animPattern3);
        List<FistBehavior> fists = new List<FistBehavior>();
        if (fistPrefab != null)
        {
            for (int i = 0; i < fistCount; i++)
            {
                float xPos = fistStartX + (i * fistSpacing);
                Vector3 spawnPos = new Vector3(xPos, fistSpawnY, 0);
                GameObject fistObj = Instantiate(fistPrefab, spawnPos, Quaternion.identity);
                FistBehavior fist = fistObj.GetComponent<FistBehavior>();
                if (fist != null)
                {
                    fists.Add(fist);
                    fist.MoveToHeight(fistReadyY, fistMoveSpeed);
                }
                yield return new WaitForSeconds(fistAppearanceInterval);
            }
        }
        yield return new WaitForSeconds(0.5f);
        for (int loop = 0; loop < repeatCount; loop++)
        {
            for (int i = 0; i < fists.Count; i++)
            {
                if (i % 2 == 0) fists[i].MoveWithVibration(fistSmashY, fistMoveSpeed, fistVibrateDuration, fistVibrateStrength);
                else fists[i].MoveToHeight(fistReadyY, fistMoveSpeed);
            }
            yield return new WaitForSeconds(smashInterval);
            for (int i = 0; i < fists.Count; i++)
            {
                if (i % 2 != 0) fists[i].MoveWithVibration(fistSmashY, fistMoveSpeed, fistVibrateDuration, fistVibrateStrength);
                else fists[i].MoveToHeight(fistReadyY, fistMoveSpeed);
            }
            yield return new WaitForSeconds(smashInterval);
        }
        foreach (var fist in fists)
        {
            if (fist != null) fist.MoveToHeight(fistSpawnY, fistMoveSpeed);
        }
        float exitWaitTime = Mathf.Abs(fistSpawnY - fistSmashY) / fistMoveSpeed;
        yield return new WaitForSeconds(exitWaitTime + 0.5f);
        foreach (var fist in fists)
        {
            if (fist != null) Destroy(fist.gameObject);
        }
    }

    IEnumerator ActionPattern4()
    {
        Debug.Log("行動4: 移動 -> 爆弾設置 -> 爆発");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern4EnemyPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern4EnemyPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animPattern4);
        if (bombPrefab != null && bombTargetPositions.Count > 0)
        {
            foreach (Vector3 targetPos in bombTargetPositions)
            {
                GameObject bombObj = Instantiate(bombPrefab, transform.position, Quaternion.identity);
                BombBehavior bomb = bombObj.GetComponent<BombBehavior>();
                if (bomb != null)
                {
                    bomb.Setup(targetPos, bombMoveDuration, bombBlinkDuration);
                }
                yield return new WaitForSeconds(bombSpawnInterval);
            }
        }
        float waitTime = bombMoveDuration + bombBlinkDuration + 1.0f;
        yield return new WaitForSeconds(waitTime);
        Debug.Log("行動4完了");
    }
}