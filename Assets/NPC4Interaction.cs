using UnityEngine;
using System.Collections;
using TMPro;

public class NPC4Interaction : MonoBehaviour
{
    #region 내부 변수
    public GameObject DialoguePanel;
    public TextMeshProUGUI npcText;
    public string[] npcDialogues;
    public float typingSpeed = 0.1f;
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    // 타이핑
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    #endregion

    void Start()
    {
        DialoguePanel.SetActive(false);
    }

    void Update()
    {
        // 플레이어가 범위 내에 있고 스페이스바를 누르면 대화 시작
        if (playerInRange && Input.GetKeyDown(KeyCode.Space) && !isDialogueActive)
        {
            StartDialogue();
        }
        // "그렇지 않고, 만약" 대화가 활성화된 상태라면 아래 로직 실행
        else if (isDialogueActive)
        {
            // ESC 키로 대화 종료
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
            // 스페이스바를 눌렀을 때 (타이핑 스킵 기능만 남김)
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                // 타이핑 중인 경우에만 즉시 완료
                if (isTyping)
                {
                    CompleteTyping();
                }
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
            EndDialogue();
            Debug.Log("플레이어가 NPC 범위에서 벗어남");
        }
    }
    #endregion

    #region 대화 로직
    private void StartDialogue()
    {
        if (npcDialogues.Length == 0 || string.IsNullOrEmpty(npcDialogues[0]))
        {
            Debug.LogError("NPC 대사가 없습니다!");
            return;
        }

        isDialogueActive = true;
        DialoguePanel.SetActive(true);
        StartTyping(npcDialogues[0]);
    }

    private void EndDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping = false;
        }
        isDialogueActive = false;
        DialoguePanel.SetActive(false);
    }

    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        npcText.text = npcDialogues[0];
        isTyping = false;
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        npcText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            npcText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
    #endregion
}