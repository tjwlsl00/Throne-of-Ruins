using UnityEngine;
using TMPro;
using System.Collections;

public class NPCDialogue : MonoBehaviour
{
    #region 내부 변수
    public GameObject DialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject playerDialoguePanel;
    public TextMeshProUGUI playerText;
    private bool playerInRange = false;
    private bool isDialoguePanel = false;
    // 대사 관련
    public string[] npcDialogues;
    public string[] playerDialogues;
    private int currentDialogueIndex = 0;
    private bool isNPCTurn = true;
    // 타이핑 효과 
    public float typingSpeed = 0.05f; // 약간 빠르게 조정
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    #endregion

    public void Start()
    {
        DialoguePanel.SetActive(false);
        playerDialoguePanel.SetActive(false);
    }

    public void Update()
    {
        // 대화창이 열려있지 않으면 스페이스바/ESC 입력을 받지 않음
        if (!isDialoguePanel)
        {
            // 대화 시작 조건
            if (playerInRange && Input.GetKeyDown(KeyCode.Space))
            {
                StartDialogue();
            }
            return; // 대화 중이 아니면 아래 로직 실행 안 함
        }

        // --- 대화가 진행 중일 때만 아래 로직 실행 ---

        // 1. 대화 진행 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 타이핑 스킵
                SkipTyping();
            }
            else
            {
                // 다음 대사로
                ShowNextDialogue();
            }
        }
        // 2. 대화 중단 (ESC)
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    #region 충돌 이벤트
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // 대화 중에 범위를 벗어나면 대화창을 강제 종료
            if (isDialoguePanel)
            {
                EndDialogue();
            }
        }
    }
    #endregion

    #region 대화 로직
    void StartDialogue()
    {
        isDialoguePanel = true;
        currentDialogueIndex = 0;
        isNPCTurn = true;
        DisplayDialogue();
    }

    private void DisplayDialogue()
    {
        // 기존 코루틴이 있다면 확실하게 종료
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (isNPCTurn)
        {
            // NPC 턴
            if (currentDialogueIndex < npcDialogues.Length)
            {
                DialoguePanel.SetActive(true);
                playerDialoguePanel.SetActive(false);
                typingCoroutine = StartCoroutine(TypeText(dialogueText, npcDialogues[currentDialogueIndex]));
            }
            else
            {
                EndDialogue(); // 표시할 대사가 없으면 종료
            }
        }
        else
        {
            // 플레이어 턴
            if (currentDialogueIndex < playerDialogues.Length)
            {
                DialoguePanel.SetActive(false);
                playerDialoguePanel.SetActive(true);
                typingCoroutine = StartCoroutine(TypeText(playerText, playerDialogues[currentDialogueIndex]));
            }
            else
            {
                EndDialogue(); // 표시할 대사가 없으면 종료
            }
        }
    }

    private void ShowNextDialogue()
    {
        // 턴 전환
        isNPCTurn = !isNPCTurn;

        // 플레이어 턴이 끝났을 때만 다음 대사 세트로 이동
        if (isNPCTurn)
        {
            currentDialogueIndex++;
        }

        DisplayDialogue();
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (isNPCTurn)
        {
            dialogueText.text = npcDialogues[currentDialogueIndex];
        }
        else
        {
            if (currentDialogueIndex < playerDialogues.Length)
                playerText.text = playerDialogues[currentDialogueIndex];
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        // 코루틴이 실행 중일 수 있으므로 안전하게 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        isTyping = false;
        DialoguePanel.SetActive(false);
        playerDialoguePanel.SetActive(false);
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
        typingCoroutine = null; // 코루틴이 끝나면 참조를 비워줌
    }
    #endregion
}