using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour
{
    #region 싱글턴 및 기본 설정
    public static PlayerMovement Instance { get; private set; }
    public static string nextSceneSpawnPointID = null;

    // 오디오 효과
    public AudioSource audioSource;
    public AudioClip attackEffect;
    public AudioClip JumpEffect;
    public AudioClip WalkEffect;
    public AudioClip DeadEffect;

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

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        SetupGroundAndBounds();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    #endregion

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 저장된 위치가 있으면 이동
        if (PlayerPrefs.HasKey("SpawnPosX"))
        {
            Vector3 spawnPos = new Vector3(PlayerPrefs.GetFloat("SpawnPosX"), PlayerPrefs.GetFloat("SpawnPosY"), PlayerPrefs.GetFloat("SPawnPosZ"));

            transform.position = spawnPos;
            Debug.Log($"포탈 위치에서 스폰: {spawnPos}");

            //데이터 초기화 
            PlayerPrefs.DeleteKey("SpawnPosX");
            PlayerPrefs.DeleteKey("SpawnPosY");
            PlayerPrefs.DeleteKey("SPawnPosZ");
        }

        if (animator != null)
        {
            animator.Play("Idle");
        }
    }

    #region 애니메이션 관련
    [Header("애니메이션 파라미터")]
    public string moveAnimParam = "isMoving";
    public string jumpAnimTrigger = "JumpTrigger";
    public string attackAnimTrigger = "AttackTrigger";
    public string deadAnimTrigger = "isDead";
    #endregion

    #region 플레이어 이동 
    [Header("이동 설정")]
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public GameObject groundObject;

    private Vector2 moveInput;
    private bool isGrounded = false;
    public bool isMoving = false;
    private bool isJumping = false;
    private float minX, maxX;

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // 스프라이트 방향 전환
        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }

        moveInput = new Vector2(horizontalInput, 0).normalized;

        // 이동 상태 업데이트
        if (!isJumping && !isAttacking)
        {
            bool wasMoving = isMoving;
            isMoving = Mathf.Abs(moveInput.x) > 0.1f;

            if (isMoving != wasMoving)
            {
                animator.SetBool(moveAnimParam, isMoving);
            }
        }

        if (isMoving && !isJumping)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = WalkEffect;
                audioSource.loop = true;
                audioSource.pitch = 1.8f;
                audioSource.Play();
            }
        }
        else // 조건 불만족 시 즉시 중지
        {
            if (audioSource.isPlaying && audioSource.clip == WalkEffect)
            {
                audioSource.Stop();
            }
        }

        // 점프 소리 처리 (기존 로직 유지)
        if (isJumping)
        {
            audioSource.PlayOneShot(JumpEffect); // OneShot으로 겹침 방지
        }
    }

    private void ApplyMovement()
    {
        if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void SetupGroundAndBounds()
    {
        if (groundObject == null) return;

        Collider2D collider = groundObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
        }
    }

    private void ClampPosition()
    {
        if (groundObject != null && (minX != 0 || maxX != 0))
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minX, maxX),
                transform.position.y,
                transform.position.z
            );
        }
    }
    #endregion

    #region 플레이어 점프 
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && isGrounded && !isAttacking)
        {
            isJumping = true;
            // isMoving = false; 뭔가 애니메이션 부자연스러움
            animator.SetBool(moveAnimParam, false);
            animator.SetTrigger(jumpAnimTrigger);

            // 오디오 연결
            audioSource.clip = JumpEffect;
            audioSource.loop = false;
            audioSource.pitch = 1.0f;
            audioSource.Play();
        }
    }

    private void ApplyJump()
    {
        if (isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = false;
        }
    }
    #endregion

    #region 플레이어 공격 
    [Header("공격 설정")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime;
    private bool isAttacking = false;
    public float attackRange = 4f;

    // --- statManger관리 위한 변수 선언 ---
    public int initialSTR;
    [HideInInspector] public int spentSTRPoints = 0;

    //공격 시작
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) &&
            Time.time >= lastAttackTime + attackCooldown &&
            !isAttacking)
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            animator.SetTrigger(attackAnimTrigger);

            // 이동 중지
            isMoving = false;
            animator.SetBool(moveAnimParam, false);

            if (isAttacking)
            {
                // 오디오 
                audioSource.clip = attackEffect;
                audioSource.loop = false;
                audioSource.pitch = 1.0f;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    // 공격 종료
    public void EndAttack()
    {
        isAttacking = false;
        Debug.Log("공격 종료 - 이동 가능"); // (디버깅용)
    }

    // 공격 범위 내 적 체크 (태그 사용)
    public void OnAttackAnimationEvent()
    {
        // 1. 모든 적 오브젝트 찾기 (태그로 필터링)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 2. 범위 내 적이 있는지 확인
        bool hitEnemy = false;
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= attackRange)
            {
                ApplyDamage(enemy);
                hitEnemy = true;
            }
        }

        if (!hitEnemy)
        {
            Debug.Log("공격 범위 내 적 없음");
        }
    }

    // 적에게 데미지 
    public void ApplyDamage(GameObject enemy)
    {
        if (enemy != null)
        {
            // Wizard
            Wizard wizard = enemy.GetComponent<Wizard>();
            if (wizard != null)
            {
                wizard.TakeDamage(PlayerState.Instance.attackForce);
                Debug.Log($"{enemy.name}에게 {PlayerState.Instance.attackForce} 데미지!");
            }

            // Goblin
            Goblin goblin = enemy.GetComponent<Goblin>();
            if (goblin != null)
            {
                goblin.TakeDamage(PlayerState.Instance.attackForce);
                Debug.Log($"{enemy.name}에게 {PlayerState.Instance.attackForce} 데미지!");
            }

            // Boss
            Boss boss = enemy.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(PlayerState.Instance.attackForce);
                Debug.Log($"{enemy.name}에게 {PlayerState.Instance.attackForce} 데미지!");
            }
        }
    }
    #endregion

    #region 플레이어 죽음
    public bool isDead = false;
    public float DeadDuration = 1f;
    public float DeadPanelActive = 1f;
    public void DieAnimation()
    {

        // 이동 중지
        isMoving = false;
        animator.SetBool(moveAnimParam, false);

        isDead = true;
        Debug.Log("사망 애니메이션 재생");

        // 플레이어 죽음 애니메이션 재생 
        animator.SetTrigger(deadAnimTrigger);

        // 오디오 연결
        audioSource.clip = DeadEffect;
        audioSource.loop = false;
        audioSource.pitch = 1.0f;
        audioSource.Play();

        // 애니메이션 종료 후 패널 등장
        StartCoroutine(isDeadPanelActive());
    }

    // public void DisablePlayerInput()
    // {
    //     // 키 입력 비활성화 방법 1: PlayerController 컴포넌트 비활성화
    //     PlayerMovement controller = GetComponent<PlayerMovement>();
    //     if (controller != null)
    //     {
    //         controller.enabled = false;
    //     }
    // }

    IEnumerator isDeadPanelActive()
    {
        yield return new WaitForSeconds(DeadDuration);
        UIManager.Instance.isDeadPanel.SetActive(true);
    }

    public Vector3 GetSpawnPosition()
    {
        // 마을에서의 기본 리스폰 위치 반환
        return new Vector3((float)-4.47, (float)2.88, 0);
    }

    #endregion

    #region 물리 및 충돌 처리
    private Rigidbody2D rb;
    private Animator animator;

    void Update()
    {
        // 죽은 상태면 플레이어 이동 입력 무시 
        if (PlayerState.Instance.isDead == true)
        {
            return;
        }
        else
        {
            HandleMovement();
            HandleJump();
            HandleAttack();
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
        ClampPosition();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            if (isJumping)
            {
                isJumping = false;
                isMoving = Mathf.Abs(moveInput.x) > 0.1f;
                animator.SetBool(moveAnimParam, isMoving);
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    #endregion

    #region 디버그 시각화(Gizmo)
    void OnDrawGizmosSelected()
    {
        DrawAttackRange();
    }

    void DrawAttackRange()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}