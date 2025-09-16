using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    #region 내부 변수 
    // [1] 상태 및 기본 설정
    private int phase = 1;
    public TextMeshProUGUI phaseText;
    private bool movingLeft = true;
    public bool isDead = false;

    // [2] 인스펙터 설정 (공개 변수)
    [Header("이동 설정")]
    public float patrolRange = 10f;
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    public float attackRange = 2f;

    [Header("참조 오브젝트")]
    public Transform player;
    public Image healthBarImage;
    public TextMeshProUGUI BossHealthText;

    [Header("애니메이션 파라미터")]
    public string patrolAnimParam = "isPatrolling";
    public string attackAnimTrigger = "AttackTrigger";
    public string dieAnimTrigger = "DieTrigger";

    [Header("적 스탯 설정")]
    public int maxHp = 50;
    private int currentHP;
    public float attackForce = 10f;

    // [3] 내부 변수 (Private)
    private Vector3 initialPosition;
    private float initialY;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private float distanceToPlayer;
    //파티클
    public ParticleSystem DeadEffect;
    // 오디오 
    public AudioSource audioSource;
    public AudioClip TakeDamageSound;
    public AudioClip BossDeadSound;
    #endregion

    #region 피격 효과 설정
    public Color flashColor = Color.black;
    public float flashDuration = 0.1f;
    public int numberOfFlashes = 3;
    public Color originalColor;
    #endregion

    // 싱글톤 정의 
    public static Boss Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // [4] 초기화
    void Start()
    {
        currentHP = maxHp;
        UpdateBossHealthUI();

        rb = GetComponent<Rigidbody2D>();
        InitializeComponents();
        SetInitialPositions();

        // phase텍스트 ui 
        phaseText.text = "phase:" + phase.ToString();
        StartCoroutine(Phase1Behavior());

        // 피격 및 죽음 파티클 효과
        originalColor = spriteRenderer.color;
        DeadEffect.Stop();
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void SetInitialPositions()
    {
        initialPosition = transform.position;
        initialY = initialPosition.y;
    }

    // [5] 메인 업데이트
    void Update()
    {
        if (player == null) return;
        UpdateDistanceToPlayer();
        UpdateBossHealthUI();

        // phase텍스트 ui 
        phaseText.text = "phase:" + phase.ToString();

    }

    void UpdateBossHealthUI()
    {
        // HP 텍스트 업데이트 
        float hpRatio = (float)currentHP / maxHp * 100f;
        BossHealthText.text = $"{currentHP} / {maxHp} ({hpRatio:F0}%)";

        // HP 바 업데이트 
        healthBarImage.fillAmount = (float)currentHP / maxHp;
    }

    void UpdateDistanceToPlayer()
    {
        if (player != null)
            distanceToPlayer = Vector2.Distance(transform.position, player.position);
    }

    // [6] Phase 1: 패트롤 & 공격
    IEnumerator Phase1Behavior()
    {
        yield return new WaitForSeconds(0.5f);
        while (phase == 1 && !isDead)
        {
            Patrol();
            // 체력이 700 이하일 때 페이즈 전환
            if (currentHP <= 700)
            {
                phase = 2;
                StartCoroutine(Phase2Behavior());
                yield break;  // 현재 코루틴 종료
            }

            if (distanceToPlayer < attackRange)
            {
                Attack();
                yield return new WaitForSeconds(2.5f);
            }
            else
            {
                yield return null;
            }
        }
    }
    //===============================================
    public Vector3 targetPosition = new Vector3(21, -6, 0); // 목표 위치
    public GameObject warningEffectPrefab;
    public GameObject fallingObjectPrefab;
    public Transform[] spawnPoints;
    public int SpawnCount = 4;
    public float warningDuration = 3f;
    public float spawnInterval = 10f;

    IEnumerator Phase2Behavior()
    {

        transform.position = targetPosition;
        yield return new WaitForSeconds(1.0f);
        Debug.Log("보스 체력 70%이하, Phase2로 이동합니다.");

        while (phase == 2 && !isDead)
        {
            animator.SetBool(patrolAnimParam, false);
            spriteRenderer.flipX = player.position.x < transform.position.x;

            // 1.랜덤 스폰 지점 선택
            HashSet<int> selectedIndices = new HashSet<int>();
            while (selectedIndices.Count < SpawnCount)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                selectedIndices.Add(randomIndex);
            }

            foreach (int index in selectedIndices)
            {
                StartCoroutine(SpawnAtPoint(spawnPoints[index].position));
            }

            // 2. 다음 패턴 대기 
            yield return new WaitForSeconds(spawnInterval);

            if (currentHP <= 400)
            {
                phase = 3;
                StartCoroutine(Phase3Behavior());
                yield break;  // 현재 코루틴 종료
            }

            if (distanceToPlayer < attackRange)
            {
                Attack();
                yield return new WaitForSeconds(2.5f);
            }

            else
            {
                yield return null;
            }
        }

        // 경고 이펙트 -> 오브젝트 낙하 로직
        IEnumerator SpawnAtPoint(Vector3 position)
        {
            //경고 이펙트 생성
            GameObject warning = Instantiate(warningEffectPrefab, position, Quaternion.identity);
            Destroy(warning, warningDuration);

            //경고 후 오브젝트 낙하 
            yield return new WaitForSeconds(warningDuration);
            Instantiate(fallingObjectPrefab, position + Vector3.up * 20f, Quaternion.identity);
        }

    }
    //===============================================
    // 몹 소환 관련 변수 
    public GameObject[] enemyPrefabs;
    public List<Transform> EnemyspawnPoints;
    public float enemySpawnInterval = 10f;
    public int maxEnemiesCount = 4;

    private List<GameObject> activeEnemies = new List<GameObject>();

    public void SpawnEnemy(Transform EnemyspawnPoint)
    {
        if (enemyPrefabs != null && EnemyspawnPoint != null)
        {
            // 등록된 프리팹에서 하나 선택
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject prefabToSpawn = enemyPrefabs[randomIndex];

            GameObject newEnemy = Instantiate(prefabToSpawn, EnemyspawnPoint.position, EnemyspawnPoint.rotation);
            activeEnemies.Add(newEnemy);
        }
        else
        {
            Debug.Log("연결된 몬스터 프리팹이 없습니다.");
        }
    }

    public void EnemyDied(GameObject deadEnemy)
    {
        if (activeEnemies.Contains(deadEnemy))
        {
            activeEnemies.Remove(deadEnemy.gameObject);
            Debug.Log($"적 제거됨. 현재 활성 적 수: {activeEnemies.Count}");
        }
    }

    IEnumerator Phase3Behavior()
    {
        yield return new WaitForSeconds(2.0f);
        Debug.Log("보스 체력 40%이하, Phase3로 이동합니다.");

        #region Phase3 패턴(몬스터 소환-> 코르틴 중복 방지 외부 에서 선언 =  한번만 실행)
        // 몬스터 소환
        StartCoroutine(SpawnEnemyCoroutine());
        #endregion

        while (phase == 3 && !isDead)
        {
            if (distanceToPlayer < attackRange)
            {
                Attack();
                yield return new WaitForSeconds(2.5f);
            }
            else
            {
                yield return null;
            }

            #region Phase2 패턴 
            animator.SetBool(patrolAnimParam, false);
            spriteRenderer.flipX = player.position.x < transform.position.x;

            // 1.랜덤 스폰 지점 선택
            HashSet<int> selectedIndices = new HashSet<int>();
            while (selectedIndices.Count < SpawnCount)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                selectedIndices.Add(randomIndex);
            }

            foreach (int index in selectedIndices)
            {
                StartCoroutine(SpawnAtPoint(spawnPoints[index].position));
            }

            // 경고 이펙트 -> 오브젝트 낙하 로직
            IEnumerator SpawnAtPoint(Vector3 position)
            {
                //경고 이펙트 생성
                GameObject warning = Instantiate(warningEffectPrefab, position, Quaternion.identity);
                Destroy(warning, warningDuration);

                //경고 후 오브젝트 낙하 
                yield return new WaitForSeconds(warningDuration);
                Instantiate(fallingObjectPrefab, position + Vector3.up * 20f, Quaternion.identity);
            }

            // 2. 다음 패턴 대기 
            yield return new WaitForSeconds(spawnInterval);
            #endregion
        }

    }
    // 몬스터 소환(코르틴 함수)
    IEnumerator SpawnEnemyCoroutine()
    {
        while (phase == 3 && !isDead)
        {
            // null 참조 정리 
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                }
            }

            if (activeEnemies.Count < maxEnemiesCount)
            {
                // 최대 수 넘지 않게 계산
                int enemiesToSpawn = Mathf.Min(maxEnemiesCount - activeEnemies.Count, EnemyspawnPoints.Count);

                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    SpawnEnemy(EnemyspawnPoints[i]);
                }
            }
            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }
    //===============================================
    void Patrol()
    {
        // 이동 방향 결정
        float direction = movingLeft ? -1f : 1f;
        spriteRenderer.flipX = movingLeft;
        animator.SetBool(patrolAnimParam, true);

        float newX = transform.position.x + direction * patrolSpeed * Time.deltaTime;

        // 패트롤 범위 제한
        if (newX < initialPosition.x - patrolDistance)
        {
            newX = initialPosition.x - patrolDistance;
            movingLeft = false;
        }
        else if (newX > initialPosition.x + patrolDistance)
        {
            newX = initialPosition.x + patrolDistance;
            movingLeft = true;
        }

        transform.position = new Vector3(newX, initialY, initialPosition.z);
    }

    void Attack()
    {
        StartCoroutine(RepeatAttack());
    }

    IEnumerator RepeatAttack()
    {
        spriteRenderer.flipX = player.position.x < transform.position.x;
        animator.SetTrigger(attackAnimTrigger);
        yield return new WaitForSeconds(2.5f);
    }
    //===============================================
    #region 체력 및 데미지 처리 
    public void ApplyDamage()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerState playerState = player.GetComponent<PlayerState>();
            if (playerState != null) playerState.TakeDamage((int)attackForce);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        UpdateBossHealthUI();
        PlayTakeDamageSound();

        if (currentHP <= 0) Die();
    }

    public void PlayTakeDamageSound()
    {
        audioSource.clip = TakeDamageSound;
        audioSource.Play();
        StartCoroutine(FlashEffectCoroutine());
    }

    IEnumerator FlashEffectCoroutine()
    {
        // 현재 알파 값 보존
        float originalAlpha = spriteRenderer.color.a;

        for (int i = 0; i < numberOfFlashes; i++)
        {
            // 알파 값을 유지한 채로 색상만 변경
            spriteRenderer.color = new Color(flashColor.r, flashColor.g, flashColor.b, originalAlpha);
            yield return new WaitForSeconds(flashDuration);

            // 원래 색상으로 복원 (알파 값 유지)
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalAlpha);
            yield return new WaitForSeconds(flashDuration);
        }

        // 최종적으로 원래 색상과 알파 값 모두 복원
        spriteRenderer.color = originalColor;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("보스가 죽었습니다.");

        // 보스 죽으면 몬스터 같이 없애기
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();

        // Die애니메이션을 위해 코르틴 사용
        DeadEffect.Play();
        animator.SetTrigger(dieAnimTrigger);
        PlayDieSound();
    }

    public void PlayDieSound()
    {
        audioSource.clip = BossDeadSound;
        audioSource.Play();
        StartCoroutine(DieAnimationEnd());
    }

    IEnumerator DieAnimationEnd()
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
        gameObject.SetActive(false);
        DeadEffect.Stop();
    }

    
    #endregion
    // [7] 디버그 시각화
    void OnDrawGizmosSelected()
    {
        DrawPatrolRange();
        DrawAttackRange();
        DrawPatrolPath();
    }
    void DrawPatrolRange()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }
    void DrawAttackRange()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    void DrawPatrolPath()
    {
        Gizmos.color = Color.green;
        Vector3 leftPatrolPoint, rightPatrolPoint;

        if (Application.isPlaying)
        {
            leftPatrolPoint = new Vector3(initialPosition.x - patrolDistance, initialY, initialPosition.z);
            rightPatrolPoint = new Vector3(initialPosition.x + patrolDistance, initialY, initialPosition.z);
        }
        else
        {
            leftPatrolPoint = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
            rightPatrolPoint = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);
        }

        Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
        Gizmos.DrawSphere(leftPatrolPoint, 0.2f);
        Gizmos.DrawSphere(rightPatrolPoint, 0.2f);
    }
}