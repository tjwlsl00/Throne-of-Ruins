using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    #region [플레이어 참조 변수]
    public PlayerState playerState; // 플레이어의 상태 정보 (체력, 마나, 경험치 등)
    #endregion

    // 레벨업 UI 요소 
    public TextMeshProUGUI playerLevelText;

    #region [체력 UI 요소]
    public Image hpFillImage;     // 체력 게이지 (Image 컴포넌트)
    public TextMeshProUGUI hpText; // 체력 수치 텍스트
    #endregion

    #region [마나 UI 요소]
    public Image mpFillImage;     // 마나 게이지
    public TextMeshProUGUI mpText; // 마나 수치 텍스트
    #endregion

    #region [경험치 UI 요소]
    public Image expFillImage;    // 경험치 게이지
    public TextMeshProUGUI expText; // 경험치 수치 텍스트
    #endregion

    #region [인벤토리 요소]
    // 인벤토리 UI 패널 연결
    public GameObject inventoryPanel;
    private bool isInventoryOpen = false;
    #endregion

    // 플레이어 죽음 패널,버튼 연결 
    public GameObject isDeadPanel;
    public Button reviveButton;

    #region [초기 설정]

    void Awake()
    {
        // 인스턴스 선언
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

    void Start()
    {
        // 플레이어 참조 초기화
        InitializePlayerReference();
        // UI 전체 업데이트
        UpdateAllUI();
        // 시작 시 인벤토리 UI 비활성화
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        // 시작 시 플레이어 isDead 패널 비활성화
        if (isDeadPanel != null && isDeadPanel.activeSelf)
            isDeadPanel.SetActive(false);
        // 부활 버튼 누르면 마을로 씬 이동
        reviveButton.onClick.AddListener(reviveToVilliage);
    }

    // 플레이어 오브젝트 찾기
    void InitializePlayerReference()
    {
        if (playerState == null)
        {
            playerState = FindAnyObjectByType<PlayerState>();
            if (playerState == null)
            {
                Debug.LogError("플레이어 상태 컴포넌트를 찾을 수 없습니다!");
                enabled = false; // 이 스크립트 비활성화
            }
        }
    }
    #endregion

    #region [UI 업데이트 로직]
    void Update()
    {
        UpdateAllUI(); // 간단한 구현을 위해 매 프레임 UI 갱신

        // i 키 입력 감지
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel != null)
            {
                ToggleInventory();
            }
        }
    }

    // 모든 UI 요소 업데이트
    void UpdateAllUI()
    {
        UpdatePlayerLevelUI();
        UpdateHPUI();  // 체력 UI 업데이트
        UpdateMPUI();  // 마나 UI 업데이트
        UpdateEXPUI(); // 경험치 UI 업데이트
    }

    // 레벨 UI 업데이트 
    public void UpdatePlayerLevelUI()
    {
        int currentLevel = playerState.playerLevel;
        playerLevelText.text = "Lv." + currentLevel;
    }

    // 체력 UI 업데이트
    public void UpdateHPUI()
    {
        if (hpFillImage != null && playerState.maxHP > 0)
            hpFillImage.fillAmount = (float)playerState.currentHP / playerState.maxHP;

        if (hpText != null)
            hpText.text = $"{playerState.currentHP}/{playerState.maxHP}";
    }

    // 마나 UI 업데이트
    void UpdateMPUI()
    {
        if (mpFillImage != null && playerState.maxMP > 0)
            mpFillImage.fillAmount = (float)playerState.currentMP / playerState.maxMP;

        if (mpText != null)
            mpText.text = $"{playerState.currentMP}/{playerState.maxMP}";
    }

    // 경험치 UI 업데이트
    void UpdateEXPUI()
    {
        if (expFillImage != null && playerState.maxEXP > 0)
            expFillImage.fillAmount = (float)playerState.currentEXP / playerState.maxEXP;

        if (expText != null)
            expText.text = $"{playerState.currentEXP}/{playerState.maxEXP}";
    }

    // 인벤토리 UI
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);

            // (선택 사항) 인벤토리가 열릴 때/닫힐 때 다른 동작을 수행할 수 있습니다.
            if (isInventoryOpen)
            {
                Debug.Log("인벤토리가 열렸습니다.");
            }
            else
            {
                Debug.Log("인벤토리가 닫혔습니다.");
            }
        }
    }
    #endregion

    // 부활버튼 클릭 -> 마을로 부활(Viliage)
    public void reviveToVilliage()
    {
        if (playerState == null)
        {
            Debug.LogError("PlayerState 참조가 없습니다!");
            return;
        }

        Debug.Log("부활버튼 클릭! 마을에서 부활합니다.");

        // 사망 패널 비활성화
        isDeadPanel.SetActive(false);

        // 마을 씬 로드
        SceneManager.LoadScene("Viliage");

        // 플레이어 상태 초기화
        playerState.currentHP = playerState.maxHP;
        playerState.isDead = false;

        // 플레이어 오브젝트 활성화 및 위치 재설정
        PlayerMovement.Instance.gameObject.SetActive(true);
        PlayerMovement.Instance.transform.position = PlayerMovement.Instance.GetSpawnPosition();

        // UI 갱신
        UpdateAllUI();
    }
}