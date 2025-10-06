using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPC2Interaction : MonoBehaviour
{
    #region 내부 변수 
    public GameObject interactionPanel;
    public Button TalkButton;
    public Button ShopOpenButton;
    public GameObject DialoguePanel;
    private bool isDialoguePanel = false;
    // 대사 관련
    public TextMeshProUGUI npcText;
    public string[] npcDialogues;
    private int currentDialogueIndex = 0;
    // 상점 연결 
    public EquipmentShopManager equipmentShopManager;
    private bool playerInRange = false;
    // 타이핑 효과 
    public float typingSpeed = 0.1f;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    #endregion

    void Start()
    {
        if (interactionPanel == null)
        {
            Debug.LogError("Interaction Panel이 할당되지 않았습니다. Inspector에서 할당해주세요.");
            return;
        }

        if (TalkButton != null)
        {
            TalkButton.onClick.AddListener(OnTalkButtonClicked);
        }

        if (ShopOpenButton != null)
        {
            ShopOpenButton.onClick.AddListener(OnShopButtonClicked);
        }

        // 패널 비활성화 된 상태에서 시작
        HideInteractionPanel();
        HideDialoguePanel();
    }

    void Update()
    {
        // 플레이어가 범위 내에 있고, 스페이스바를 눌렀을 때, 그리고 '대화창이 꺼져 있을 때만'
        if (playerInRange && Input.GetKeyDown(KeyCode.Space) && !isDialoguePanel)
        {
            if (!interactionPanel.activeSelf)
            {
                ShowInteractionPanel();
            }
            else
            {
                HideInteractionPanel();
            }
        }

        // 대화 버튼 클릭 후 상태에서 ESC누르면 창 닫기 
        if (isDialoguePanel)
        {
            // 타이핑 중에 스페이스바를 누르면
            if (isTyping && Input.GetKeyDown(KeyCode.Space))
            {
                // 타이핑 중이면 즉시 완성
                StopCoroutine(typingCoroutine);
                npcText.text = npcDialogues[currentDialogueIndex];
                isTyping = false;
            }
            // ESC를 누르면
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideDialoguePanel();
            }
        }
    }

    #region 충돌 이벤트 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("플레이어가 NPC 범위에 들어왔습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("플레이어가 NPC 범위에서 벗어남");
        }
    }
    #endregion

    #region 패널 open 및 hide    
    public void ShowInteractionPanel()
    {
        interactionPanel.SetActive(true);
        Debug.Log("상호작용 패널 활성화");
    }

    public void HideInteractionPanel()
    {
        interactionPanel.SetActive(false);
        Debug.Log("상호작용 패널 비활성화");
    }

    public void OnTalkButtonClicked()
    {
        Debug.Log("대화하기 버튼 클릭!");
        HideInteractionPanel(); // 대화 시작 후 패널 닫기
        ShowDialoguePanel();
    }

    public void OnShopButtonClicked()
    {
        Debug.Log("상점 버튼 클릭!");
        equipmentShopManager.ToggleShop();
        HideInteractionPanel(); // 상점 연 후 패널 닫기
    }

    // Dialoguage 패널 open, hide
    public void ShowDialoguePanel()
    {
        // 만약 이미 실행 중인 타이핑 코루틴이 있다면 즉시 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        DialoguePanel.SetActive(true);
        isDialoguePanel = true;
        currentDialogueIndex = 0; // 대화를 항상 처음부터 시작
                                  // 새로운 코루틴을 시작하고, 그 참조를 저장
        typingCoroutine = StartCoroutine(TypeText(npcText, npcDialogues[currentDialogueIndex]));
    }

    public void HideDialoguePanel()
    {
        // 패널을 닫을 때도 만약을 위해 코루틴을 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        DialoguePanel.SetActive(false);
        isDialoguePanel = false;
    }

    // 타이핑 효과 
    IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
    #endregion
}
