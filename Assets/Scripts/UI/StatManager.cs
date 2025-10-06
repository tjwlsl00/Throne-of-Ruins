using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum StatType { HP, MP, STR }

public class StatManager : MonoBehaviour
{
    #region [플레이어 참조 변수]
    public PlayerState playerState; // 플레이어의 상태 정보 
    #endregion

    public TextMeshProUGUI availableStatPointsText;

    // --- UI 텍스트 요소 (유니티 에디터에서 연결) ---
    [Header("Stat Text Displays")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI strText;
    public TextMeshProUGUI availablePointsText;

    // --- UI 버튼 요소 (유니티 에디터에서 연결) ---
    [Header("Stat Buttons")]
    public Button hpStatUpButton;
    public Button mpStatUpButton;
    public Button strStatUpButton;
    public Button resetButton; // 리셋 버튼

    // --- 스탯 포인트당 증가량 (에디터에서 설정 가능) ---
    [Header("Increase Per Point")]
    public int hpIncreaseAmount = 5;
    public int mpIncreaseAmount = 5;
    public int strIncreaseAmount = 1;

    // 버튼 효과음
    public AudioClip buttonEffect;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateStatUI();
    }

    public void UpdateStatUI()
    {
        hpText.text = playerState.maxHP.ToString();
        mpText.text = playerState.maxMP.ToString();
        strText.text = playerState.attackForce.ToString();
        // 스탯 포인트 
        int CanUseStatPoint = playerState.availableStatPoints;
        availableStatPointsText.text = CanUseStatPoint.ToString();
    }

    public bool IncreaseStat(StatType stat)
    {
        if (playerState == null)
        {
            Debug.Log("PlayerState 참조 미설정");
            return false;
        }
        if (playerState.availableStatPoints <= 0)
        {
            Debug.Log("사용 가능한 스탯 포인트가 부족합니다.");
            return false;
        }

        bool success = false;

        switch (stat)
        {
            case StatType.HP:
                // HP 값 증가
                playerState.maxHP += hpIncreaseAmount;
                // 사용된 포인트 카운트
                playerState.spentHPPoints++;
                // 성공 표시
                success = true;
                break;

            case StatType.MP:
                playerState.maxMP += mpIncreaseAmount;
                playerState.spentMPPoints++;
                success = true;
                break;

            case StatType.STR:
                playerState.attackForce += strIncreaseAmount;
                playerState.spentSTRPoints++;
                success = true;
                break;
            default:
                Debug.LogWarning($"알 수 없는 스탯 타입: {stat}");
                break;
        }

        // 스탯 증가 시 포인트 1 소모 
        if (success)
        {
            playerState.availableStatPoints--;
            GetStatValue(stat);
            UpdateAvaliablePointsDisplay(playerState.availableStatPoints);
        }
        return success;
    }

    public void ResetStats()
    {
        // 사용했던 스탯 포인트 모두 반환
        playerState.availableStatPoints += playerState.spentHPPoints;
        playerState.availableStatPoints += playerState.spentMPPoints;
        playerState.availableStatPoints += playerState.spentSTRPoints;

        // 스탯 값 초기화
        playerState.maxHP = playerState.initialHP;  // 최대 체력 초기화
        playerState.maxMP = playerState.initialMP;  // 최대 마력 초기화
        playerState.attackForce = playerState.initialSTR;  // 공격력 초기화

        // 사용된 포인트 카운터 초기화
        playerState.spentHPPoints = 0;
        playerState.spentMPPoints = 0;
        playerState.spentSTRPoints = 0;

        Debug.Log("스탯이 초기화되었습니다.");

    }

    // 특정 스탯 타입에 해당하는 현재 값을 반환하는 헬퍼 메서드
    private int GetStatValue(StatType stat)
    {
        if (playerState == null) return 0;
        switch (stat)
        {
            case StatType.HP: return playerState.currentHP;
            case StatType.MP: return playerState.currentMP;
            case StatType.STR: return playerState.attackForce;
            default: return 0;
        }
    }

    // 사용 가능한 스탯 포인트 UI 텍스트를 업데이트
    private void UpdateAvaliablePointsDisplay(int newPoints)
    {
        if (availableStatPointsText != null)
        {
            availableStatPointsText.text = newPoints.ToString();
        }
    }


    public void OnClickHPUp()
    {
        IncreaseStat(StatType.HP);
        PlaybuttonEffect();
    }

    public void OnClickMPUp()
    {
        IncreaseStat(StatType.MP);
        PlaybuttonEffect();
    }

    public void OnClickSTRUp()
    {
        IncreaseStat(StatType.STR);
        PlaybuttonEffect();
    }

    public void OnClickReset()
    {
        ResetStats();
        PlaybuttonEffect();
    }

    public void PlaybuttonEffect()
    {
        audioSource.clip = buttonEffect;
        audioSource.Play();
    }

}
