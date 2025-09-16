using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    #region 플레이어 스테이터스 관련 수치 변수 
    public int maxHP = 100;
    public int currentHP = 100;

    public int maxMP = 50;
    public int currentMP = 50;

    public int attackForce = 10;

    public int playerLevel = 1;
    public int currentEXP = 0;
    public int maxEXP = 100;
    public int availableStatPoints = 0;

    // 플레이어 돈 
    public int money = 10;

    // --- 초기 스탯 값 (리셋 시 사용) ---
    public int initialHP;
    public int initialMP;
    public int initialSTR;

    // --- 각 스탯에 사용된 포인트(리셋 시 포인트 반환용) ---
    [HideInInspector] public int spentHPPoints = 0;
    [HideInInspector] public int spentMPPoints = 0;
    [HideInInspector] public int spentSTRPoints = 0;

    #endregion

    #region 피격 효과 설정
    public Color flashColor = Color.black;
    public float flashDuration = 0.1f;
    public int numberOfFlashes = 3;
    public SpriteRenderer spriteRenderer;
    public Color originalColor;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 플레이어 statManager 참조 변수(초기값)
        initialHP = maxHP;
        initialMP = maxMP;
        initialSTR = attackForce;
        availableStatPoints = 0;

        // 플레이어 피격 관련
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

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
         if (scene.name == "Village")
    {
        // 죽은 상태에서 마을로 이동했을 때만 체력 초기화
        if (isDead)
        {
            Debug.Log("마을 씬 로드 완료 (죽음 상태). 플레이어 HP를 초기화합니다.");
            currentHP = maxHP;
            isDead = false;
            
            // 플레이어 오브젝트 활성화
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.gameObject.SetActive(true);
                PlayerMovement.Instance.isDead = false;
                PlayerMovement.Instance.transform.position = PlayerMovement.Instance.GetSpawnPosition();
            }
        }
    }
    }

    #region 플레이어 MP 소비 
    public void UseMP(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0)
        {
            currentMP = 0;
        }
    }
    #endregion

    #region 플레이어 레벨업

    public void GainExp(int amount)
    {
        currentEXP += amount;

        while (currentEXP >= maxEXP)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        playerLevel++;
        availableStatPoints += 5;
        currentEXP -= maxEXP;
        Debug.Log("플레이어 레벨업!");
        Debug.Log("사용할 수 있는 스탯 포인트:" + availableStatPoints);
    }

    #endregion

    #region 플레이어 데미지 입음

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        StartCoroutine(FlashEffectCoroutine());

        if (currentHP <= 0)
        {
            Die();
        }
    }
    // 깜빡임 효과(코루틴 함수)
    IEnumerator FlashEffectCoroutine()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        spriteRenderer.color = originalColor;
    }
    #endregion

    #region 플레이어 회복
    public void Heal(int amount)
    {
        if (currentHP >= maxHP)
        {
            Debug.Log("현재 플레이어의 체력이 풀입니다.");
            return;
        }

        currentHP = Mathf.Min(currentHP + amount, maxHP);
        Debug.Log($"체력 회복! (+{amount}) 현재 체력: {currentHP}/{maxHP}");

    }

    #endregion

    #region 플레이어 죽음 
    public bool isDead = false;
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        isDead = true;
        PlayerMovement.Instance.DieAnimation();
    }

    public bool IsDead()
    {
        return isDead;
    }
    #endregion

    #region 장비 장착 체력 증가 
    // public 메서드로 추가 (필수) - 장비 장착/ 해제 상태에 따라 효과 적용
    public void ModifyMaxHP(int amount)
    {
        Debug.Log($"MaxHP 변경 전: {maxHP}, 변경량: {amount}");
        maxHP += amount;
        currentHP = Mathf.Clamp(currentHP, 1, maxHP);
        Debug.Log($"MaxHP 변경 후: {maxHP}, CurrentHP: {currentHP}");
    }
    #endregion

    #region 플레이어 돈 관련 로직 
    public void Addmoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("음수 금액을 추가할 수 없습니다. 돈을 차감하려면 RemoveMoney를 사용하세요.");
            return;
        }
        money += amount;

    }

    public bool RemoveMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("음수 금액을 차감할 수 없습니다.");
            return false;
        }
        if (money >= amount)
        {
            money -= amount;
            return true; // 성공적으로 차감
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            return false; // 돈 부족
        }
    }
    #endregion
}
