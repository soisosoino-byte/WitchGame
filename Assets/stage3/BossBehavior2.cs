using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior2 : MonoBehaviour
{
    [Header("基本設定")]
    public float startWaitTime = 2.0f;
    public float intervalTime = 1.5f;

    [Header("オーディオ設定")]
    private AudioSource audioSource;

    [Tooltip("パターン1: 手裏剣の発射音")]
    public AudioClip soundPattern1Shot;
    [Range(0, 1)] public float volumePattern1 = 0.5f; // 音量スライダー

    [Tooltip("パターン2: 雑魚の召喚音")]
    public AudioClip soundSummonMinion;
    [Range(0, 1)] public float volumeSummon = 1.0f;

    [Tooltip("パターン3: 障害物を投げる音")]
    public AudioClip soundThrow;
    [Range(0, 1)] public float volumeThrow = 1.0f;

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
    public float chargeTime = 0.5f;
    public string animPattern1Attack = "RapidFire";
    public float fireDuration = 3.0f;
    public float fireInterval = 0.08f;

    // --- パターン2設定 ---
    [Header("パターン2の設定")]
    public GameObject minionPrefab;
    public Vector3 pattern2TargetPos = new Vector3(0, 3, 0);
    public string animPattern2 = "Chant";

    // --- パターン3設定 ---
    [Header("パターン3の設定")]
    public GameObject obstaclePrefab;
    public GameObject healItemPrefab;
    public Vector3 pattern3TargetPos = new Vector3(0, 3, 0);
    public string animPattern3 = "Attack";
    [Header("投擲設定")]
    public int totalThrowCount = 5;
    public float throwInterval = 0.5f;
    public Vector2 throwForce = new Vector2(-5f, 8f);

    // --- パターン4設定 ---
    [Header("パターン4の設定（10秒経過イベント）")]
    public GameObject thornPrefab;
    public GameObject dustPrefab;
    public Vector3 pattern4EnemyPos = new Vector3(0, 2, 0);
    public string animPattern4 = "Attack";

    [Header("茨の設定")]
    public Vector3 thorn1StartPos = new Vector3(-8, -2, 0);
    public Vector3 thorn1EndPos = new Vector3(8, -2, 0);
    public Vector3 thorn2StartPos = new Vector3(-8, 0, 0);
    public Vector3 thorn2EndPos = new Vector3(8, 0, 0);
    public float thornMoveDuration = 4.0f;

    [Tooltip("茨の振動の横幅")]
    public float thornVibration = 0.5f;

    [Header("土煙の設定")]
    public Vector3 dustSpawnPos1 = new Vector3(-5, -4, 0);
    public Vector3 dustSpawnPos2 = new Vector3(5, -4, 0);

    [Header("アニメーション名設定")]
    public string animMove = "Move";
    public string animCharge = "Charge";
    public string animFire = "Attack";
    public string animIdle = "Idle";

    // 内部変数
    private int lastPattern = -1;
    private int bannedPattern = -1;
    private int banTurnCount = 0;
    private float sceneTimer = 0f;
    private bool hasExecutedPattern4 = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(BattleLoop());
    }

    void Update()
    {
        sceneTimer += Time.deltaTime;
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
        if (sceneTimer >= 10.0f && !hasExecutedPattern4)
        {
            hasExecutedPattern4 = true;
            return 4;
        }

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
        Debug.Log("ボス2: 行動1（連射）");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern1TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern1TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = pattern1TargetPos;

        if (animator != null) animator.Play(animCharge);
        yield return new WaitForSeconds(chargeTime);

        if (animator != null) animator.Play(animPattern1Attack);
        float timer = 0f;
        while (timer < fireDuration)
        {
            if (attack1Prefab != null)
            {
                Instantiate(attack1Prefab, transform.position, Quaternion.identity);
                // ★音再生（音量指定）
                if (soundPattern1Shot != null) audioSource.PlayOneShot(soundPattern1Shot, volumePattern1);
            }
            yield return new WaitForSeconds(fireInterval);
            timer += fireInterval;
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ActionPattern2()
    {
        Debug.Log("ボス2: 行動2（雑魚召喚）");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern2TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern2TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animPattern2);

        // ★音再生（召喚時）
        if (soundSummonMinion != null) audioSource.PlayOneShot(soundSummonMinion, volumeSummon);

        yield return new WaitForSeconds(1.0f);

        if (minionPrefab != null)
        {
            Vector3 spawnPos1 = transform.position + new Vector3(-1.0f, 0, 0);
            GameObject zako1 = Instantiate(minionPrefab, spawnPos1, Quaternion.identity);
            MinionBehavior mb1 = zako1.GetComponent<MinionBehavior>();
            if (mb1 != null) mb1.direction = -1;

            Vector3 spawnPos2 = transform.position + new Vector3(1.0f, 0, 0);
            GameObject zako2 = Instantiate(minionPrefab, spawnPos2, Quaternion.identity);
            MinionBehavior mb2 = zako2.GetComponent<MinionBehavior>();
            if (mb2 != null) mb2.direction = 1;
        }
        yield return new WaitForSeconds(2.0f);
    }

    IEnumerator ActionPattern3()
    {
        Debug.Log("ボス2: 行動3（投擲）");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern3TargetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern3TargetPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (animator != null) animator.Play(animPattern3);

        int healIndex = Random.Range(0, totalThrowCount);
        for (int i = 0; i < totalThrowCount; i++)
        {
            GameObject objToSpawn = (i == healIndex) ? healItemPrefab : obstaclePrefab;
            if (objToSpawn != null)
            {
                GameObject obj = Instantiate(objToSpawn, transform.position, Quaternion.identity);
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 randomForce = throwForce;
                    randomForce.x += Random.Range(-1f, 1f);
                    randomForce.y += Random.Range(-0.5f, 1f);
                    rb.AddForce(randomForce, ForceMode2D.Impulse);
                }
                // ★音再生（投げる音）
                if (soundThrow != null) audioSource.PlayOneShot(soundThrow, volumeThrow);
            }
            yield return new WaitForSeconds(throwInterval);
        }
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator ActionPattern4()
    {
        Debug.Log("行動4: 茨の襲撃");
        if (animator != null) animator.Play(animMove);
        while (Vector3.Distance(transform.position, pattern4EnemyPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pattern4EnemyPos, enemyMoveSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.Play(animPattern4);

        GameObject dust1 = null;
        GameObject dust2 = null;
        if (dustPrefab != null)
        {
            dust1 = Instantiate(dustPrefab, dustSpawnPos1, Quaternion.identity);
            dust2 = Instantiate(dustPrefab, dustSpawnPos2, Quaternion.identity);
        }

        if (thornPrefab != null)
        {
            GameObject t1 = Instantiate(thornPrefab, thorn1StartPos, Quaternion.identity);
            ThornBehavior tb1 = t1.GetComponent<ThornBehavior>();
            if (tb1 != null) tb1.Setup(thorn1StartPos, thorn1EndPos, thornMoveDuration, thornVibration);

            GameObject t2 = Instantiate(thornPrefab, thorn2StartPos, Quaternion.identity);
            ThornBehavior tb2 = t2.GetComponent<ThornBehavior>();
            if (tb2 != null) tb2.Setup(thorn2StartPos, thorn2EndPos, thornMoveDuration, thornVibration);
        }

        yield return new WaitForSeconds(thornMoveDuration);

        if (dust1 != null) Destroy(dust1);
        if (dust2 != null) Destroy(dust2);

        yield return new WaitForSeconds(1.0f);
        Debug.Log("行動4完了");
    }
}