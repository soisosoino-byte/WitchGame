using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq; // LINQを使用するため追加

public class EnemyAttackManager : MonoBehaviour
{
    [Serializable]
    public class AttackPatternInfo
    {
        public string patternName;
        [Range(0, 100)]
        public int weight;
    }

    [Serializable]
    public class Chain2SpecificSettings
    {
        public string name = "Chain2 Instance";
        public Vector3 initialSpawnPosition = new Vector3(0, 0, 0);
        public Vector3 targetPosition = new Vector3(2, -3, 0);
        public float initialDelayBeforeMove = 1.0f;
        public float moveDuration = 0.7f;
        [Range(-180f, 180f)]
        public float initialRotationZ = 0f;
        public float delayBeforeFade = 2.0f;
        public float fadeDuration = 0.5f;
        [Range(0.01f, 2.0f)]
        public float fadeInDuration = 0.2f;

        [Header("Effect Settings (After Chain2 Move)")]
        public bool spawnEffect = false;
        public Vector3 effectSpawnOffset = Vector3.zero;
        [Range(1, 10)]
        public int effectCount = 1;
        [Range(0.01f, 0.5f)]
        public float effectInterval = 0.1f;
        [Range(0.05f, 1.0f)]
        public float effectFadeOutDuration = 0.2f;
        [Range(0.1f, 3.0f)]
        public float effectScale = 1.0f;
    }

    [Serializable]
    public class Chain2GroupSettings
    {
        public string groupName = "Chain2 Group";
        public float delayBeforeGroupSpawn = 0f;
        public List<Chain2SpecificSettings> chain2sInGroup = new List<Chain2SpecificSettings>();
    }

    [Serializable]
    public class IndividualBeemSettings
    {
        public string name = "Beem Instance";
        public Vector3 initialSpawnPosition = new Vector3(0, 0, 0);
        public Vector3 targetPosition = new Vector3(0, -5, 0);
        [Range(-180f, 180f)]
        public float initialRotationZ = 0f;
        [Range(-180f, 180f)]
        public float targetRotationZ = 0f;
        public float moveDuration = 1.0f;
        [Range(0.01f, 1.0f)]
        public float fadeInDuration = 0.3f;
        [Range(0.01f, 1.0f)]
        public float fadeOutDuration = 0.1f;
        public Vector2 pivotOffset = Vector2.zero;
    }

    [Serializable]
    public class BeemAttackStep
    {
        public string stepName = "Beem Attack Step";
        public float delayBeforeStep = 0f;
        public List<IndividualBeemSettings> beemsInThisStep = new List<IndividualBeemSettings>();
    }

    [Serializable]
    public class EnemyPhaseMovement
    {
        public string phaseName;
        public Vector3 targetPosition;
        public float moveDuration = 1.0f;
        public string animationTrigger = "Move"; // ★追加: この移動中に再生するアニメーションのトリガー名★
    }

    // ★追加: 攻撃パターンごとの敵の移動・アニメーション設定★
    [Serializable]
    public class EnemyPatternTransition
    {
        public string patternName; // 対象の攻撃パターン名 (Pattern 1, Pattern 2, Pattern 3, Pattern 4)
        public Vector3 targetPosition; // このパターン開始時の敵の目標座標
        public float moveDuration = 1.0f; // 移動にかかる時間
        public string animationTrigger = "Move"; // このパターン開始時に再生するアニメーションのトリガー名
    }

    // ★新規追加: 斬撃エフェクトの個別の設定
    [Serializable]
    public class SlashEffectSetting
    {
        public string name = "Slash Instance";
        public GameObject slashEffectPrefab; // ★修正: 個別のPrefabを設定できるように追加★
        public Vector3 spawnPosition; // 斬撃エフェクトの出現座標
        public Vector3 targetPosition; // 斬撃エフェクトの移動目標座標
        public float moveDuration = 0.1f; // 移動にかかる時間 (待機時間なしなので短い)
        public float fadeOutDuration = 0.3f; // 透明になるまでの時間
        public float initialRotationZ = 0f; // 初期回転Z
    }

    // ★新規追加: 攻撃パターン4全体の斬撃設定
    [Serializable]
    public class SlashAttackPattern4Settings
    {
        public string attackName = "Slash Attack 1";
        public float delayBeforeAttack = 0.5f; // この一連の斬撃を開始するまでの遅延
        public List<SlashEffectSetting> slashEffects = new List<SlashEffectSetting>(); // 5回の斬撃エフェクト設定
    }


    public GameObject objectToSpawnPrefab;

    [Header("Attack Pattern 1 Settings")]
    public float pattern1SpawnY = 5.0f;
    public float pattern1SpawnMinX = -8.0f;
    public float pattern1SpawnMaxX = 8.0f;
    public float pattern1Duration = 5.0f;
    public int pattern1SpawnCount = 15;

    [Header("Attack Pattern 2 Settings")]
    public GameObject chain1Prefab;
    public GameObject chain2Prefab;
    public GameObject effectPrefab;

    public float chain1SpawnX = 0.0f;
    public float chain1StartPosY = 6.0f;
    public float chain1EndPosY = 0.0f;
    public float chain1MoveSpeed = 2.0f;

    [Header("Chain2 Attack Groups")]
    public List<Chain2GroupSettings> chain2AttackGroups = new List<Chain2GroupSettings>();

    [Header("Attack Pattern 3 Settings")]
    public GameObject beemPrefab;
    public List<BeemAttackStep> beemAttackSequence = new List<BeemAttackStep>();

    // ★新規追加: Attack Pattern 4 Settings
    [Header("Attack Pattern 4 Settings")]
    // public GameObject slashEffectPrefab; // ★修正: SlashEffectSettingの中に移動したため削除★
    public List<SlashAttackPattern4Settings> slashAttackSequence = new List<SlashAttackPattern4Settings>(); // 斬撃攻撃のシーケンス


    [Header("Initial Delay")]
    public float initialAttackDelay = 5.0f;

    public float attackInterval = 3.0f;

    [Header("Attack Pattern Probabilities")]
    public List<AttackPatternInfo> attackPatterns = new List<AttackPatternInfo>();

    [Header("Enemy Base Settings")]
    public GameObject enemyBasePrefab;
    public Vector3 enemyInitialSpawnPosition = new Vector3(0, 5, 0);
    public EnemyPhaseMovement initialPhaseMovement;
    public List<EnemyPhaseMovement> intervalPhaseMovements = new List<EnemyPhaseMovement>();

    // ★追加: 攻撃パターンごとの敵の移動とアニメーション設定のリスト★
    [Header("Enemy Movement per Attack Pattern")]
    public List<EnemyPatternTransition> patternTransitions = new List<EnemyPatternTransition>();


    private bool isAttacking = false;
    private GameObject currentChain1Obj;
    private int currentChain2Count = 0;
    private int totalChain2ToDestroy = 0;
    private bool chain1Destroyed = false;
    private int currentBeemCount = 0;
    private int totalBeemToDestroy = 0;

    // ★新規追加: パターン4の斬撃エフェクトの現在カウントと合計
    private int currentSlashEffectCount = 0;
    private int totalSlashEffectsToDestroy = 0;


    private EnemyMovementController enemyMovementController;
    private GameObject currentEnemyInstance; // ★追加: 現在シーンにいる敵のインスタンスへの参照

    // ★追加: 敵が倒されたかどうかのフラグ
    private bool isDefeated = false;

    // ★新規追加ここから★
    private string lastChosenPattern = "";         // 前回選択されたパターン
    private string secondLastChosenPattern = "";   // 前々回選択されたパターン
    private int banTurnsRemaining = 0;             // 禁止残りターン数
    private const int BAN_DURATION = 2;            // 禁止するターン数
    // ★新規追加ここまで★


    void Start()
    {
        if (attackPatterns.Count == 0)
        {
            attackPatterns.Add(new AttackPatternInfo { patternName = "Pattern 1", weight = 25 }); // Weightを調整
            attackPatterns.Add(new AttackPatternInfo { patternName = "Pattern 2", weight = 25 }); // Weightを調整
            attackPatterns.Add(new AttackPatternInfo { patternName = "Pattern 3", weight = 25 }); // Weightを調整
            attackPatterns.Add(new AttackPatternInfo { patternName = "Pattern 4", weight = 25 }); // ★新規追加: Pattern 4
        }

        // --- デフォルト設定の更新 ---
        // Chain2GroupSettingsの初期設定例 (Inspectorで設定済みならこのブロックは削除/コメントアウト)
        if (chain2AttackGroups.Count == 0)
        {
            Chain2GroupSettings group1 = new Chain2GroupSettings { groupName = "First Group (3 Chains)", delayBeforeGroupSpawn = 0f };
            group1.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-1",
                initialSpawnPosition = new Vector3(-3, 6, 0),
                targetPosition = new Vector3(-3, 0, 0),
                initialDelayBeforeMove = 0.5f,
                moveDuration = 0.7f,
                initialRotationZ = 0f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0, -0.5f, 0),
                effectCount = 3,
                effectInterval = 0.08f,
                effectFadeOutDuration = 0.2f,
                effectScale = 0.8f
            });
            group1.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-2",
                initialSpawnPosition = new Vector3(0, 6, 0),
                targetPosition = new Vector3(0, 0, 0),
                initialDelayBeforeMove = 0.7f,
                moveDuration = 0.7f,
                initialRotationZ = 0f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0.2f, -0.3f, 0),
                effectCount = 3,
                effectInterval = 0.08f,
                effectFadeOutDuration = 0.2f,
                effectScale = 0.9f
            });
            group1.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-3",
                initialSpawnPosition = new Vector3(3, 6, 0),
                targetPosition = new Vector3(3, 0, 0),
                initialDelayBeforeMove = 0.9f,
                moveDuration = 0.7f,
                initialRotationZ = 0f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(-0.2f, -0.7f, 0),
                effectCount = 3,
                effectInterval = 0.08f,
                effectFadeOutDuration = 0.2f,
                effectScale = 1.0f
            });
            chain2AttackGroups.Add(group1);

            Chain2GroupSettings group2 = new Chain2GroupSettings { groupName = "Second Group (4 Chains)", delayBeforeGroupSpawn = 1.0f };
            group2.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-4",
                initialSpawnPosition = new Vector3(-4, 6, 0),
                targetPosition = new Vector3(-4, -1, 0),
                initialDelayBeforeMove = 0.5f,
                moveDuration = 0.7f,
                initialRotationZ = 45f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0, 0, 0),
                effectCount = 2,
                effectInterval = 0.1f,
                effectFadeOutDuration = 0.2f,
                effectScale = 1.2f
            });
            group2.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-5",
                initialSpawnPosition = new Vector3(-1.5f, 6, 0),
                targetPosition = new Vector3(-1.5f, -1, 0),
                initialDelayBeforeMove = 0.6f,
                moveDuration = 0.7f,
                initialRotationZ = 0f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0, 0, 0),
                effectCount = 2,
                effectInterval = 0.1f,
                effectFadeOutDuration = 0.2f,
                effectScale = 1.0f
            });
            group2.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-6",
                initialSpawnPosition = new Vector3(1.5f, 6, 0),
                targetPosition = new Vector3(1.5f, -1, 0),
                initialDelayBeforeMove = 0.7f,
                moveDuration = 0.7f,
                initialRotationZ = 0f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0, 0, 0),
                effectCount = 2,
                effectInterval = 0.1f,
                effectFadeOutDuration = 0.2f,
                effectScale = 1.0f
            });
            group2.chain2sInGroup.Add(new Chain2SpecificSettings
            {
                name = "Chain2-7",
                initialSpawnPosition = new Vector3(4, 6, 0),
                targetPosition = new Vector3(4, -1, 0),
                initialDelayBeforeMove = 0.8f,
                moveDuration = 0.7f,
                initialRotationZ = -45f,
                delayBeforeFade = 1.0f,
                fadeDuration = 0.5f,
                fadeInDuration = 0.2f,
                spawnEffect = true,
                effectSpawnOffset = new Vector3(0, 0, 0),
                effectCount = 2,
                effectInterval = 0.1f,
                effectFadeOutDuration = 0.2f,
                effectScale = 1.2f
            });
            chain2AttackGroups.Add(group2);
        }

        // BeemAttackSequenceの初期設定例 (Inspectorで設定することを推奨)
        if (beemAttackSequence.Count == 0)
        {
            BeemAttackStep step1 = new BeemAttackStep { stepName = "Single Beem 1 (Center Pivot)", delayBeforeStep = 0f };
            step1.beemsInThisStep.Add(new IndividualBeemSettings
            {
                name = "Beem 1-1",
                initialSpawnPosition = new Vector3(0, 3, 0),
                targetPosition = new Vector3(0, -3, 0),
                initialRotationZ = 0f,
                targetRotationZ = 90f,
                moveDuration = 1.0f,
                fadeInDuration = 0.3f,
                fadeOutDuration = 0.1f,
                pivotOffset = Vector2.zero
            });
            beemAttackSequence.Add(step1);

            BeemAttackStep step2 = new BeemAttackStep { stepName = "Single Beem 2 (Left End Pivot)", delayBeforeStep = 1.0f };
            step2.beemsInThisStep.Add(new IndividualBeemSettings
            {
                name = "Beem 2-1",
                initialSpawnPosition = new Vector3(-3, 3, 0),
                targetPosition = new Vector3(3, -3, 0),
                initialRotationZ = -45f,
                targetRotationZ = 45f,
                moveDuration = 1.2f,
                pivotOffset = new Vector2(-1.0f, 0f)
            });
            beemAttackSequence.Add(step2);

            BeemAttackStep step3 = new BeemAttackStep { stepName = "Two Beems (Right End Pivot)", delayBeforeStep = 1.0f };
            step3.beemsInThisStep.Add(new IndividualBeemSettings
            {
                name = "Beem 3-1",
                initialSpawnPosition = new Vector3(4, 4, 0),
                targetPosition = new Vector3(-4, -4, 0),
                initialRotationZ = 60f,
                targetRotationZ = -60f,
                moveDuration = 1.5f,
                fadeInDuration = 0.3f,
                fadeOutDuration = 0.1f,
                pivotOffset = new Vector2(1.0f, 0f)
            });
            step3.beemsInThisStep.Add(new IndividualBeemSettings
            {
                name = "Beem 3-2",
                initialSpawnPosition = new Vector3(-4, 4, 0),
                targetPosition = new Vector3(4, -4, 0),
                initialRotationZ = -60f,
                targetRotationZ = 60f,
                moveDuration = 1.5f,
                fadeInDuration = 0.3f,
                fadeOutDuration = 0.1f,
                pivotOffset = new Vector2(1.0f, 0f)
            });
            beemAttackSequence.Add(step3);
        }

        // ★新規追加: SlashAttackPattern4Settingsの初期設定例 (Inspectorで設定することを推奨)
        // ここでは、デフォルトの斬撃Prefabを各設定に割り当てる例を示します。
        // 実際には、複数の異なる斬撃Prefabを用意して、Inspectorで設定することになります。
        if (slashAttackSequence.Count == 0)
        {
            // Debug.LogWarning("SlashEffectPrefabが設定されていません。Pattern 4の斬撃は生成されません。", this); // コメントアウト、SlashEffectSettingに移動したため
            SlashAttackPattern4Settings slashGroup1 = new SlashAttackPattern4Settings { attackName = "Basic 5 Slashes", delayBeforeAttack = 0.5f };

            // ★修正: 各SlashEffectSettingに slashEffectPrefab を設定★
            slashGroup1.slashEffects.Add(new SlashEffectSetting { name = "Slash 1", slashEffectPrefab = objectToSpawnPrefab, spawnPosition = new Vector3(-2, 0, 0), targetPosition = new Vector3(2, 0, 0), moveDuration = 0.1f, fadeOutDuration = 0.3f, initialRotationZ = 0f });
            slashGroup1.slashEffects.Add(new SlashEffectSetting { name = "Slash 2", slashEffectPrefab = objectToSpawnPrefab, spawnPosition = new Vector3(2, 0, 0), targetPosition = new Vector3(-2, 0, 0), moveDuration = 0.1f, fadeOutDuration = 0.3f, initialRotationZ = 180f });
            slashGroup1.slashEffects.Add(new SlashEffectSetting { name = "Slash 3", slashEffectPrefab = objectToSpawnPrefab, spawnPosition = new Vector3(0, 2, 0), targetPosition = new Vector3(0, -2, 0), moveDuration = 0.1f, fadeOutDuration = 0.3f, initialRotationZ = -90f });
            slashGroup1.slashEffects.Add(new SlashEffectSetting { name = "Slash 4", slashEffectPrefab = objectToSpawnPrefab, spawnPosition = new Vector3(-3, 1, 0), targetPosition = new Vector3(3, -1, 0), moveDuration = 0.1f, fadeOutDuration = 0.3f, initialRotationZ = 45f });
            slashGroup1.slashEffects.Add(new SlashEffectSetting { name = "Slash 5", slashEffectPrefab = objectToSpawnPrefab, spawnPosition = new Vector3(3, 1, 0), targetPosition = new Vector3(-3, -1, 0), moveDuration = 0.1f, fadeOutDuration = 0.3f, initialRotationZ = -45f });
            slashAttackSequence.Add(slashGroup1);

            // 例: 別の斬撃Prefabを使う設定（Inspectorで適切なPrefabを設定してください）
            // GameObject anotherSlashPrefab = Resources.Load<GameObject>("Prefabs/AnotherSlashEffect"); // 例: Resourcesフォルダからロード
            // if (anotherSlashPrefab != null) {
            //    SlashAttackPattern4Settings slashGroup2 = new SlashAttackPattern4Settings { attackName = "Advanced Slashes", delayBeforeAttack = 1.0f };
            //    slashGroup2.slashEffects.Add(new SlashEffectSetting { name = "Advanced Slash 1", slashEffectPrefab = anotherSlashPrefab, spawnPosition = new Vector3(0, 3, 0), targetPosition = new Vector3(0, -3, 0), moveDuration = 0.2f, fadeOutDuration = 0.5f, initialRotationZ = 90f });
            //    slashAttackSequence.Add(slashGroup2);
            // }
        }

        // EnemyPatternTransition の初期設定例
        if (patternTransitions.Count == 0)
        {
            patternTransitions.Add(new EnemyPatternTransition { patternName = "Pattern 1", targetPosition = new Vector3(0, 3, 0), moveDuration = 1.0f, animationTrigger = "Pattern1Move" });
            patternTransitions.Add(new EnemyPatternTransition { patternName = "Pattern 2", targetPosition = new Vector3(-4, 2, 0), moveDuration = 1.5f, animationTrigger = "Pattern2Move" });
            patternTransitions.Add(new EnemyPatternTransition { patternName = "Pattern 3", targetPosition = new Vector3(4, 2, 0), moveDuration = 1.2f, animationTrigger = "Pattern3Move" });
            patternTransitions.Add(new EnemyPatternTransition { patternName = "Pattern 4", targetPosition = new Vector3(0, 1, 0), moveDuration = 0.8f, animationTrigger = "Attack" }); // ★新規追加: Pattern 4
        }
        // --- デフォルト設定の更新 ここまで ---


        if (enemyBasePrefab != null)
        {
            currentEnemyInstance = Instantiate(enemyBasePrefab, enemyInitialSpawnPosition, Quaternion.identity); // ★変更: currentEnemyInstanceに割り当て
            enemyMovementController = currentEnemyInstance.GetComponent<EnemyMovementController>(); // ★変更: currentEnemyInstanceから取得
            if (enemyMovementController == null)
            {
                Debug.LogError("EnemyBasePrefabにEnemyMovementControllerスクリプトが見つかりません！", this);
            }
        }
        else
        {
            Debug.LogWarning("EnemyBasePrefabが設定されていません。敵本体は生成されません。", this);
        }

        StartCoroutine(ManageAttackPatterns());
    }

    // ★追加: 外部から攻撃を停止させるメソッド
    public void StopAttackBehavior()
    {
        isDefeated = true; // 敵が倒れたことを示すフラグを立てる
        StopAllCoroutines(); // 現在実行中のすべてのコルーチンを停止
        Debug.Log("EnemyAttackManager: 攻撃行動を停止しました。");

        // 敵の移動も完全に停止させる (EnemyMovementControllerにStopMovement()などがあれば呼ぶ)
        if (enemyMovementController != null)
        {
            enemyMovementController.StopAllCoroutines(); // 移動コルーチンも停止
            enemyMovementController.SetIdle(); // 倒れた時にIdleアニメーションに戻しておく
        }
    }


    IEnumerator ManageAttackPatterns()
    {
        Debug.Log($"シーン開始から {initialAttackDelay} 秒後に最初の攻撃を開始します...");

        // ★追加: 敵が倒されたらコルーチンを抜ける
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、ManageAttackPatternsを終了します。"); yield break; }

        if (enemyMovementController != null && initialPhaseMovement != null) // initialPhaseMovement.moveDuration > 0 のチェックはSetTriggerAndMoveToPosition内でやるのがより適切
        {
            // シーン開始時の敵の移動アニメーション
            Debug.Log($"敵が初期フェーズの目標座標 {initialPhaseMovement.targetPosition} に移動開始。アニメーション: {initialPhaseMovement.animationTrigger}");
            yield return StartCoroutine(enemyMovementController.SetTriggerAndMoveToPosition(initialPhaseMovement.targetPosition, initialPhaseMovement.moveDuration, initialPhaseMovement.animationTrigger));
            Debug.Log($"敵が初期フェーズの目標座標 {initialPhaseMovement.targetPosition} に移動しました。");
        }
        else
        {
            // Debug.LogWarning("EnemyMovementControllerまたは初期フェーズ移動設定が不十分です。初期移動をスキップします。", this);
            // 何もしない場合もIdleアニメーションにしておく
            if (enemyMovementController != null) enemyMovementController.SetIdle();
        }

        yield return new WaitForSeconds(initialAttackDelay);
        Debug.Log("最初の攻撃遅延が終了しました。攻撃パターン管理を開始します。");

        int currentIntervalPhaseIndex = 0;

        // ★変更: isDefeatedがtrueになったらループを抜ける
        while (!isDefeated)
        {
            AttackPatternInfo selectedPattern = ChooseAttackPattern();
            if (selectedPattern != null)
            {
                EnemyPatternTransition transition = patternTransitions.Find(t => t.patternName == selectedPattern.patternName);

                // ★追加: 敵が倒されたら、移動コルーチンを開始しない
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、パターン遷移をスキップします。"); yield break; }

                if (transition != null && enemyMovementController != null)
                {
                    Debug.Log($"敵が攻撃パターン '{selectedPattern.patternName}' 用に移動を開始します。目標座標: {transition.targetPosition}, アニメーション: {transition.animationTrigger}");
                    yield return StartCoroutine(enemyMovementController.SetTriggerAndMoveToPosition(transition.targetPosition, transition.moveDuration, transition.animationTrigger));
                    Debug.Log($"敵が攻撃パターン '{selectedPattern.patternName}' 用の目標座標に移動完了しました。");
                }
                else if (enemyMovementController != null)
                {
                    // 特定のパターン遷移設定がない場合、待機アニメーションを確実に設定
                    enemyMovementController.SetIdle();
                }
            }
            else
            {
                Debug.LogError("攻撃パターンが選択されませんでした。ロジックを確認してください。", this);
                yield break; // エラーの場合はループを抜ける
            }

            // ★追加: 敵が倒されたら、攻撃パターンを実行しない
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、攻撃パターン実行をスキップします。"); yield break; }

            // 選択された攻撃パターンを実行
            yield return ExecuteAttackPattern(selectedPattern);

            // ★追加: 敵が倒されたら、Chain1の待機をスキップ (Pattern 2固有)
            if (selectedPattern.patternName == "Pattern 2" && currentChain1Obj != null)
            {
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Chain1待機をスキップします。"); yield break; }

                chain1Destroyed = false;
                Chain1Behavior chain1Behavior = currentChain1Obj.GetComponent<Chain1Behavior>();
                if (chain1Behavior != null)
                {
                    chain1Behavior.OnChain1Destroyed += OnChain1Destroyed;
                }
                yield return new WaitUntil(() => chain1Destroyed || isDefeated); // ★変更: isDefeatedでも抜ける
                if (chain1Behavior != null)
                {
                    chain1Behavior.OnChain1Destroyed -= OnChain1Destroyed;
                }
                currentChain1Obj = null;
            }


            Debug.Log($"次の攻撃まで {attackInterval} 秒待機します...");

            // ★追加: 敵が倒されたら、インターバル移動をスキップ
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、インターバル移動をスキップします。"); yield break; }

            if (enemyMovementController != null && intervalPhaseMovements.Count > 0)
            {
                EnemyPhaseMovement nextMove = intervalPhaseMovements[currentIntervalPhaseIndex];
                Debug.Log($"敵がパターン間移動 '{nextMove.phaseName}' を開始します。目標座標: {nextMove.targetPosition}, アニメーション: {nextMove.animationTrigger}");
                yield return StartCoroutine(enemyMovementController.SetTriggerAndMoveToPosition(nextMove.targetPosition, nextMove.moveDuration, nextMove.animationTrigger));
                Debug.Log($"敵がパターン間移動 '{nextMove.phaseName}' の目標座標 {nextMove.targetPosition} に移動しました。");

                currentIntervalPhaseIndex = (currentIntervalPhaseIndex + 1) % intervalPhaseMovements.Count;
            }
            else if (enemyMovementController != null)
            {
                enemyMovementController.SetIdle();
            }

            // ★追加: 敵が倒されたら、次の攻撃までの待機をスキップ
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、次の攻撃までの待機をスキップします。"); yield break; }
            yield return new WaitForSeconds(attackInterval);
        }
        Debug.Log("EnemyAttackManager: ManageAttackPatternsが終了しました (敵が倒されたため)。");
    }

    private void OnChain1Destroyed()
    {
        chain1Destroyed = true;
        Debug.Log("Chain1が完全に消失しました。");
    }

    private AttackPatternInfo ChooseAttackPattern()
    {
        // 禁止ターンが残っている場合、ターン数を減らす
        if (banTurnsRemaining > 0)
        {
            banTurnsRemaining--;
            Debug.Log($"パターン '{secondLastChosenPattern}' の禁止残りターン数: {banTurnsRemaining}");
        }

        List<AttackPatternInfo> availablePatterns = new List<AttackPatternInfo>(attackPatterns);

        // 同じパターンが2連続したばかりで、まだ禁止期間中の場合、そのパターンを除外する
        if (banTurnsRemaining > 0 && !string.IsNullOrEmpty(secondLastChosenPattern))
        {
            availablePatterns.RemoveAll(p => p.patternName == secondLastChosenPattern);
            Debug.Log($"禁止中のパターン '{secondLastChosenPattern}' を選択肢から除外しました。");
        }

        int totalWeight = 0;
        foreach (var pattern in availablePatterns)
        {
            totalWeight += pattern.weight;
        }

        if (totalWeight == 0)
        {
            Debug.LogWarning("利用可能な攻撃パターンの重みが0です。攻撃が実行されません。", this);
            return null;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeightSum = 0;
        AttackPatternInfo selectedPattern = null;

        foreach (var pattern in availablePatterns)
        {
            currentWeightSum += pattern.weight;
            if (randomValue < currentWeightSum)
            {
                selectedPattern = pattern;
                break;
            }
        }

        // パターン履歴を更新
        if (selectedPattern != null)
        {
            // 2回連続で同じパターンが選ばれた場合
            if (selectedPattern.patternName == lastChosenPattern && !string.IsNullOrEmpty(lastChosenPattern))
            {
                // 禁止開始
                banTurnsRemaining = BAN_DURATION;
                Debug.Log($"パターン '{selectedPattern.patternName}' が2連続しました！今後 {BAN_DURATION} ターンの間、このパターンは禁止されます。");
            }
            // 履歴をシフト
            secondLastChosenPattern = lastChosenPattern;
            lastChosenPattern = selectedPattern.patternName;
            Debug.Log($"選ばれたパターン: {selectedPattern.patternName}. 前回: {secondLastChosenPattern}, 今回: {lastChosenPattern}");
        }

        return selectedPattern;
    }

    private IEnumerator ExecuteAttackPattern(AttackPatternInfo selectedPattern)
    {
        // ★追加: 敵が倒されたら即座に終了
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、ExecuteAttackPatternを終了します。"); yield break; }

        if (selectedPattern != null)
        {
            Debug.Log($"選択された攻撃パターン: {selectedPattern.patternName}");

            switch (selectedPattern.patternName)
            {
                case "Pattern 1":
                    yield return StartCoroutine(AttackPattern1());
                    break;
                case "Pattern 2":
                    yield return StartCoroutine(AttackPattern2());
                    break;
                case "Pattern 3":
                    yield return StartCoroutine(AttackPattern3());
                    break;
                // ★新規追加: Attack Pattern 4のケース
                case "Pattern 4":
                    yield return StartCoroutine(AttackPattern4());
                    break;
                default:
                    Debug.LogWarning($"不明な攻撃パターン名: {selectedPattern.patternName}", this);
                    break;
            }
            // 攻撃パターン完了後、必ず待機アニメーションに戻す
            if (enemyMovementController != null)
            {
                enemyMovementController.SetIdle();
            }
        }
    }


    IEnumerator AttackPattern1()
    {
        // ★追加: 敵が倒されたら即座に終了
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、AttackPattern1を終了します。"); yield break; }

        if (objectToSpawnPrefab == null)
        {
            Debug.LogWarning("生成するオブジェクトのPrefabが設定されていません！", this);
            yield break;
        }

        isAttacking = true;
        float timePerSpawn = pattern1Duration / pattern1SpawnCount;

        for (int i = 0; i < pattern1SpawnCount; i++)
        {
            // ★追加: 敵が倒されたらループを抜ける
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、AttackPattern1の生成を中断します。"); break; }

            float randomX = UnityEngine.Random.Range(pattern1SpawnMinX, pattern1SpawnMaxX);
            Vector3 spawnPosition = new Vector3(randomX, pattern1SpawnY, 0);
            Instantiate(objectToSpawnPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"オブジェクトを生成しました: {spawnPosition}");
            if (i < pattern1SpawnCount - 1)
            {
                yield return new WaitForSeconds(timePerSpawn);
            }
        }
        isAttacking = false;
        Debug.Log("攻撃パターン1が完了しました。");
    }

    IEnumerator AttackPattern2()
    {
        // ★追加: 敵が倒されたら即座に終了
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、AttackPattern2を終了します。"); yield break; }

        if (chain1Prefab == null || chain2Prefab == null)
        {
            Debug.LogWarning("攻撃パターン2に必要なPrefabが設定されていません！", this);
            yield break;
        }
        if (chain2AttackGroups.Count == 0)
        {
            Debug.LogWarning("Chain2のグループ設定がありません。Chain2が生成されません。", this);
            yield break;
        }

        isAttacking = true;

        Vector3 chain1SpawnPos = new Vector3(chain1SpawnX, chain1StartPosY, 0);
        currentChain1Obj = Instantiate(chain1Prefab, chain1SpawnPos, Quaternion.identity);
        Chain1Behavior chain1Behavior = currentChain1Obj.GetComponent<Chain1Behavior>();

        if (chain1Behavior != null)
        {
            chain1Behavior.SetupChain1(chain1EndPosY, chain1MoveSpeed);

            // ★変更: 敵が倒されたら待機を抜ける
            yield return new WaitUntil(() => chain1Behavior.IsAtEndPosition() || isDefeated);
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Chain1処理を中断します。"); yield break; }


            foreach (var group in chain2AttackGroups)
            {
                // ★追加: 敵が倒されたらグループの処理をスキップ
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Chain2グループ処理を中断します。"); break; }

                if (group.delayBeforeGroupSpawn > 0)
                {
                    Debug.Log($"次のChain2グループ '{group.groupName}' の出現まで {group.delayBeforeGroupSpawn} 秒待機します。");
                    yield return new WaitForSeconds(group.delayBeforeGroupSpawn);
                }

                currentChain2Count = 0;
                totalChain2ToDestroy = group.chain2sInGroup.Count;

                Debug.Log($"Chain2グループ '{group.groupName}' ({totalChain2ToDestroy}個) を生成します。");

                foreach (var config in group.chain2sInGroup)
                {
                    // ★追加: 敵が倒されたら個々のChain2の生成をスキップ
                    if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、個々のChain2生成を中断します。"); break; }

                    GameObject chain2Obj = Instantiate(chain2Prefab, config.initialSpawnPosition, Quaternion.Euler(0, 0, config.initialRotationZ));
                    Chain2Behavior chain2Behavior = chain2Obj.GetComponent<Chain2Behavior>();

                    if (chain2Behavior != null)
                    {
                        chain2Behavior.SetupChain2(
                            config.initialSpawnPosition,
                            config.targetPosition,
                            config.initialDelayBeforeMove,
                            config.moveDuration,
                            config.delayBeforeFade,
                            config.fadeDuration,
                            config.initialRotationZ,
                            config.fadeInDuration,
                            config.spawnEffect ? effectPrefab : null,
                            config.effectSpawnOffset,
                            config.effectCount,
                            config.effectInterval,
                            config.effectFadeOutDuration,
                            config.effectScale
                        );
                        chain2Behavior.OnChain2Destroyed += OnChain2Destroyed;
                    }
                    else
                    {
                        Debug.LogWarning($"生成されたChain2オブジェクトにChain2Behaviorが見つかりません: {chain2Obj.name}", chain2Obj);
                        currentChain2Count++; // Behaviorがない場合もカウントを進める
                    }
                }

                // グループ内のChain2が全て消えるまで待機 (または敵が倒されるまで)
                yield return new WaitUntil(() => currentChain2Count >= totalChain2ToDestroy || isDefeated);
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Chain2グループの待機を中断します。"); break; }
                Debug.Log($"Chain2グループ '{group.groupName}' のChain2が全て消失しました。");
            }
        }
        else
        {
            Debug.LogWarning("Chain1Behaviorが見つかりません。", currentChain1Obj);
        }

        isAttacking = false;
        Debug.Log("攻撃パターン2が完了しました。");
    }

    private void OnChain2Destroyed()
    {
        currentChain2Count++;
        Debug.Log($"Chain2が一つ消失しました。現在の消失数: {currentChain2Count}/{totalChain2ToDestroy}");
    }

    IEnumerator AttackPattern3()
    {
        // ★追加: 敵が倒されたら即座に終了
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、AttackPattern3を終了します。"); yield break; }

        if (beemPrefab == null)
        {
            Debug.LogWarning("BeemPrefabが設定されていません！", this);
            yield break;
        }
        if (beemAttackSequence.Count == 0)
        {
            Debug.LogWarning("Beem攻撃シーケンスが設定されていません。", this);
            yield break;
        }

        isAttacking = true;

        foreach (var step in beemAttackSequence)
        {
            // ★追加: 敵が倒されたらステップの処理をスキップ
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Beem攻撃シーケンスを中断します。"); break; }

            if (step.delayBeforeStep > 0)
            {
                Debug.Log($"次のBeem攻撃ステップ '{step.stepName}' まで {step.delayBeforeStep} 秒待機します。");
                yield return new WaitForSeconds(step.delayBeforeStep);
            }

            currentBeemCount = 0;
            totalBeemToDestroy = step.beemsInThisStep.Count;

            Debug.Log($"Beem攻撃ステップ '{step.stepName}' ({totalBeemToDestroy}個のBeem) を開始します。");

            foreach (var beemConfig in step.beemsInThisStep)
            {
                // ★追加: 敵が倒されたら個々のBeem生成をスキップ
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、個々のBeem生成を中断します。"); break; }

                GameObject beemObj = Instantiate(beemPrefab, beemConfig.initialSpawnPosition, Quaternion.identity); // BeemParentの初期位置
                BeemBehavior beemBehavior = beemObj.GetComponent<BeemBehavior>();

                if (beemBehavior != null)
                {
                    beemBehavior.SetupBeem(
                        beemConfig.initialSpawnPosition,
                        beemConfig.targetPosition,
                        beemConfig.initialRotationZ,
                        beemConfig.targetRotationZ,
                        beemConfig.moveDuration,
                        beemConfig.fadeInDuration,
                        beemConfig.fadeOutDuration,
                        beemConfig.pivotOffset
                    );
                    beemBehavior.OnBeemDestroyed += OnBeemDestroyed;
                }
                else
                {
                    Debug.LogWarning($"生成されたBeemオブジェクトにBeemBehaviorが見つかりません: {beemObj.name}", beemObj);
                    currentBeemCount++; // Behaviorがない場合もカウントを進める
                }
            }
            // ステップ内のBeemが全て消えるまで待機 (または敵が倒されるまで)
            yield return new WaitUntil(() => currentBeemCount >= totalBeemToDestroy || isDefeated);
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Beemステップの待機を中断します。"); break; }
            Debug.Log($"Beem攻撃ステップ '{step.stepName}' のBeemが全て消失しました。");
        }

        isAttacking = false;
        Debug.Log("攻撃パターン3が完了しました。");
    }

    private void OnBeemDestroyed()
    {
        currentBeemCount++;
        Debug.Log($"Beemが一つ消失しました。現在の消失数: {currentBeemCount}/{totalBeemToDestroy}");
    }


    // ★新規追加: Attack Pattern 4 (斬撃攻撃)
    IEnumerator AttackPattern4()
    {
        // ★追加: 敵が倒されたら即座に終了
        if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、AttackPattern4を終了します。"); yield break; }

        if (slashAttackSequence.Count == 0)
        {
            Debug.LogWarning("Slash攻撃シーケンスが設定されていません。", this);
            yield break;
        }

        isAttacking = true;

        foreach (var slashGroup in slashAttackSequence)
        {
            // ★追加: 敵が倒されたらグループの処理をスキップ
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Slash攻撃シーケンスを中断します。"); break; }

            if (slashGroup.delayBeforeAttack > 0)
            {
                Debug.Log($"次のSlash攻撃グループ '{slashGroup.attackName}' まで {slashGroup.delayBeforeAttack} 秒待機します。");
                yield return new WaitForSeconds(slashGroup.delayBeforeAttack);
            }

            currentSlashEffectCount = 0;
            totalSlashEffectsToDestroy = slashGroup.slashEffects.Count;

            Debug.Log($"Slash攻撃グループ '{slashGroup.attackName}' ({totalSlashEffectsToDestroy}個の斬撃) を開始します。");

            foreach (var slashConfig in slashGroup.slashEffects)
            {
                // ★追加: 敵が倒されたら個々のSlash生成をスキップ
                if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、個々のSlash生成を中断します。"); break; }

                if (slashConfig.slashEffectPrefab == null)
                {
                    Debug.LogWarning($"Slash Effect Prefabが設定されていません。斬撃 '{slashConfig.name}' は生成されません。", this);
                    currentSlashEffectCount++; // Prefabがない場合もカウントを進める
                    continue;
                }

                GameObject slashObj = Instantiate(slashConfig.slashEffectPrefab, slashConfig.spawnPosition, Quaternion.identity);
                SlashEffectBehavior slashBehavior = slashObj.GetComponent<SlashEffectBehavior>();

                if (slashBehavior != null)
                {
                    slashBehavior.SetupSlashEffect(
                        slashConfig.spawnPosition,
                        slashConfig.targetPosition,
                        slashConfig.moveDuration,
                        slashConfig.fadeOutDuration,
                        slashConfig.initialRotationZ
                    );
                    slashBehavior.OnSlashEffectDestroyed += OnSlashEffectDestroyed;
                }
                else
                {
                    Debug.LogWarning($"生成されたSlashオブジェクトにSlashEffectBehaviorが見つかりません: {slashObj.name}", slashObj);
                    currentSlashEffectCount++; // Behaviorがない場合もカウントを進める
                }
            }
            // グループ内の斬撃が全て消えるまで待機 (または敵が倒されるまで)
            yield return new WaitUntil(() => currentSlashEffectCount >= totalSlashEffectsToDestroy || isDefeated);
            if (isDefeated) { Debug.Log("EnemyAttackManager: 敵が倒されているため、Slashステップの待機を中断します。"); break; }
            Debug.Log($"Slash攻撃グループ '{slashGroup.attackName}' の斬撃が全て消失しました。");
        }

        isAttacking = false;
        Debug.Log("攻撃パターン4が完了しました。");
    }

    private void OnSlashEffectDestroyed()
    {
        currentSlashEffectCount++;
        Debug.Log($"Slash Effectが一つ消失しました。現在の消失数: {currentSlashEffectCount}/{totalSlashEffectsToDestroy}");
    }
}