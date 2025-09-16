using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Goblin : MonoBehaviour
{
    #region 상태 정의
    public enum EnemyState { Patrol, Chase, Attack, ReturnToPatrol }
    public EnemyState currentState = EnemyState.Patrol;
    #endregion

    #region 인스펙터 설정
    [Header("이동 설정")]
    public float patrolRange = 10f;
    public float attackRange = 2f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float returnSpeed = 3f;
    public float patrolDistance = 5f;

    [Header("참조 오브젝트")]
    public Transform player;
    public Image healthBarImage;

    [Header("애니메이션 파라미터")]
    public string patrolAnimParam = "isPatrolling";
    public string chaseAnimParam = "isChasing";
    public string attackAnimTrigger = "AttackTrigger";
    public string dieAnimTrigger = "DieTrigger";

    [Header("적 스탯 설정")]
    public float attackForce = 10f;
    public int maxHp = 50;
    public int expReward = 10;
    public GameObject moneyItemPrefab;
    public int moneyReward = 15;
    #endregion

    #region 내부 변수
    private bool movingLeft = true;
    private Vector3 initialPosition;
    private float initialY;
    private Vector3 returnStartPosition;
    private float returnProgress;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Coroutine attackCoroutine;
    private int currentHP;
    private bool isDead = false;
    private float attackInterval = 2.0f;
    private Rigidbody2D rb;
    private Transform mySpawnPoint;
    public AudioClip DeadEffect;
    private AudioSource audioSource;

    private Dungeon2Manager dungeon2Manager;
    public void SetEnemyManager(Dungeon2Manager manager)
    {
        dungeon2Manager = manager;
    }

    #endregion

    #region 피격 효과 설정
    public Color flashColor = Color.black;
    public float flashDuration = 0.1f;
    public int numberOfFlashes = 3;
    public Color originalColor;
    #endregion

    #region 초기화
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeComponents();
        SetInitialPositions();
        originalColor = spriteRenderer.color;
        audioSource = GetComponent<AudioSource>();
    }

    void Awake()
    {
        currentHP = maxHp;
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
    #endregion

    #region 메인 업데이트
    void Update()
    {
        if (player == null) return;

        if (!isDead)
        {
            MaintainYPosition();
            UpdateStateMachine();
        }
    }

    void MaintainYPosition()
    {
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);
    }

    void UpdateStateMachine()
    {
        // 플레이어가 죽었는지 확인
        bool isPlayerDead = player.GetComponent<PlayerState>().IsDead();

        if (isPlayerDead)
        {
            if (currentState != EnemyState.Patrol && currentState != EnemyState.ReturnToPatrol)
            {
                PrepareReturnToPatrol();
            }
            return;
        }

        float distanceToPlayer = CalculatePlayerDistance();

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                UpdateChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                UpdateAttackState(distanceToPlayer);
                break;
            case EnemyState.ReturnToPatrol:
                UpdateReturnState();
                break;
        }
    }

    float CalculatePlayerDistance()
    {
        // return Vector3.Distance(
        //     new Vector3(transform.position.x, 0, transform.position.z),
        //     new Vector3(player.position.x, 0, player.position.z)
        // );
        return Vector2.Distance(transform.position, player.position);
    }
    #endregion

    #region 상태별 동작
    // 1. 순찰 상태
    void UpdatePatrolState(float distanceToPlayer)
    {
        PatrolMovement();
        if (distanceToPlayer <= patrolRange) TransitionToChaseState();
    }

    void PatrolMovement()
    {
        // Debug.Log("적이 순찰 중 입니다.");
        float direction = movingLeft ? -1f : 1f;
        spriteRenderer.flipX = movingLeft;
        animator.SetBool(patrolAnimParam, true);

        float newX = transform.position.x + direction * patrolSpeed * Time.deltaTime;

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

    void TransitionToChaseState()
    {
        currentState = EnemyState.Chase;
        animator.SetBool(patrolAnimParam, false);
        animator.SetBool(chaseAnimParam, true);
    }

    // 2. 추적 상태
    void UpdateChaseState(float distanceToPlayer)
    {
        ChaseMovement();

        if (distanceToPlayer > patrolRange) PrepareReturnToPatrol();
        else if (distanceToPlayer <= attackRange) TransitionToAttackState();
    }

    void ChaseMovement()
    {
        // Debug.Log("적이 플레이어를 추적 중 입니다.");
        spriteRenderer.flipX = player.position.x < transform.position.x;
        animator.SetBool(chaseAnimParam, true);

        Vector3 targetPosition = new Vector3(player.position.x, initialY, player.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
    }

    void PrepareReturnToPatrol()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        returnStartPosition = transform.position;
        returnProgress = 0f;
        currentState = EnemyState.ReturnToPatrol;
        animator.SetBool(chaseAnimParam, false);
    }

    void TransitionToAttackState()
    {
        currentState = EnemyState.Attack;
        animator.SetBool(chaseAnimParam, false);
        animator.SetTrigger(attackAnimTrigger);
        Debug.Log("공격 상태로 전환!");
    }

    // 3. 공격 상태
    void UpdateAttackState(float distanceToPlayer)
    {
        if (attackCoroutine == null) attackCoroutine = StartCoroutine(RepeatAttack());

        if (distanceToPlayer > attackRange) ReturnToChaseState();
        else if (distanceToPlayer > patrolRange) PrepareReturnToPatrol();
    }

    IEnumerator RepeatAttack()
    {
        while (currentState == EnemyState.Attack)
        {
            animator.SetTrigger(attackAnimTrigger);
            yield return new WaitForSeconds(attackInterval);
        }
        attackCoroutine = null;
    }

    void ReturnToChaseState()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        currentState = EnemyState.Chase;
        animator.SetBool(chaseAnimParam, true);
    }

    // 4. 복귀 상태
    void UpdateReturnState()
    {
        returnProgress += returnSpeed * Time.deltaTime;

        if (returnProgress >= 1f) CompleteReturnToPatrol();

        float targetX = initialPosition.x + Mathf.Clamp(
            transform.position.x - initialPosition.x,
            -patrolDistance,
            patrolDistance
        );

        transform.position = new Vector3(
            Mathf.Lerp(returnStartPosition.x, targetX, returnProgress),
            initialY,
            Mathf.Lerp(returnStartPosition.z, initialPosition.z, returnProgress)
        );
    }

    void CompleteReturnToPatrol()
    {
        returnProgress = 1f;
        currentState = EnemyState.Patrol;
        movingLeft = (transform.position.x < initialPosition.x);
    }
    #endregion

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
        UpdateEnemyhpBar();
        StartCoroutine(FlashEffectCoroutine());

        if (currentHP <= 0) Die();
    }

    void UpdateEnemyhpBar()
    {
        healthBarImage.fillAmount = (float)currentHP / maxHp;
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
    #endregion

    #region 죽음 및 리스폰

    public void SetMySpawnPoint(Transform spawnPoint)
    {
        mySpawnPoint = spawnPoint;
    }

    public void Die()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // Dungeon1에서 죽으면 플레이어 경험치 + 돈 떨구기 
        if (currentScene == "Dungeon2")
        {
            if (isDead) return;
            isDead = true;
            animator.SetTrigger(dieAnimTrigger);
            Debug.Log("적이 죽었습니다.");

            // 플레이어 EXP
            PlayerState playerState = FindAnyObjectByType<PlayerState>();
            if (playerState != null) playerState.GainExp(expReward);

            // 돈 떨구기 
            if (moneyItemPrefab != null)
            {
                // 적의 현재 위치에 돈 아이템을 생성합니다.
                GameObject droppedMoney = Instantiate(moneyItemPrefab, transform.position, Quaternion.identity);

                // 생성된 돈 아이템의 MoneyItem 스크립트를 가져와서 값을 설정합니다.
                // 이렇게 하면 적마다 떨구는 돈의 양을 다르게 설정할 수 있습니다.
                MoneyItem moneyItem = droppedMoney.GetComponent<MoneyItem>();
                if (moneyItem != null)
                {
                    moneyItem.value = moneyReward;
                }
                else
                {
                    Debug.LogWarning("생성된 moneyItemPrefab에 MoneyItem 스크립트가 없습니다.");
                }

                // (선택 사항) 돈이 약간 튀어 오르도록 힘을 가할 수 있습니다.
                Rigidbody2D rb = droppedMoney.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float forceMagnitude = 5f;
                    Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
                    rb.AddForce(randomDirection * forceMagnitude, ForceMode2D.Impulse);
                }
            }

        }
        // BossDungeon에서 죽으면 그냥 죽기만
        else if (currentScene == "BossDungeon")
        {
            if (isDead) return;
            isDead = true;
            animator.SetTrigger(dieAnimTrigger);
            Debug.Log("적이 죽었습니다.");
        }

        // 오디오 연결
        audioSource.clip = DeadEffect;
        audioSource.Play();

        // Die애니메이션을 위해 코르틴 사용
        StartCoroutine(DieAnimationEnd());
    }

    IEnumerator DieAnimationEnd()
    {
        // 2초(Die Animation)후, enemyManager에게 죽음 전달
        yield return new WaitForSeconds(2.0f);

        if (dungeon2Manager != null && mySpawnPoint != null)
        {
            dungeon2Manager.EnemyDied(this, mySpawnPoint);
        }
        else
        {
            Destroy(gameObject);
        }

        gameObject.SetActive(false);
    }

    public void InitializeEnemy()
    {
        currentHP = maxHp;
        // 다른 필요한 초기화 (애니메이션 상태, 위치 등)
        isDead = false; //죽음 초기화 
        gameObject.SetActive(true); // 다시 활성화
        Debug.Log($"{gameObject.name} 초기화됨.");
    }
    #endregion

    #region 디버그 시각화
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
    #endregion
}