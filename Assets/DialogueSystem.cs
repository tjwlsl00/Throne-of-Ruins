using TMPro;
using UnityEngine;
using System.Collections;


public class DialogueSystem : MonoBehaviour
{
    [Header("NPC 대사")]
    public GameObject npcDialoguePanel;
    public TextMeshProUGUI npcText;

    [Header("Player 대사")]
    public GameObject playerDialoguePanel;
    public TextMeshProUGUI playerText;

    [Header("대사 콘텐츠")]
    // 각각 npc, player 대사 배열
    public string[] npcDialogues;
    public string[] playerDialogues;

    private bool isDialogueActive = false;
    private int currentDialogueIndex = 0;
    private bool isNPCTurn = true;
    private GameObject player;

    [Header("타이핑 효과")]
    public float typingSpeed = 1.2f;
    private bool isTyping = false;
    private Coroutine typingCoroutine;


    void Start()
    {
        // 플레이어 찾아서 자동 할당
        player = GameObject.FindWithTag("Player");
        npcDialoguePanel.SetActive(false);
        playerDialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 타이핑 중이면 즉시 완성
                StopCoroutine(typingCoroutine);
                if (isNPCTurn)
                {
                    npcText.text = npcDialogues[currentDialogueIndex];
                }
                else
                {
                    playerText.text = playerDialogues[currentDialogueIndex];
                }
                isTyping = false;
            }
            else
            {
                ShowNextDialogue();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDialogueActive)
        {
            Debug.Log("범위 내 플레이어 접촉 대화가능");
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        currentDialogueIndex = 0;
        isNPCTurn = true;

        // 플레이어 이동 스크립트 비활성화
        if (player != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;

                Animator animator = player.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool(playerMovement.moveAnimParam, false);
                    playerMovement.audioSource.Stop();
                }
            }
        }
        ShowCurrentDialogue();
    }

    void ShowCurrentDialogue()
    {
        if (isNPCTurn && currentDialogueIndex < npcDialogues.Length)
        {
            // NPC 대사 표시 
            npcDialoguePanel.SetActive(true);
            playerDialoguePanel.SetActive(false);
            typingCoroutine = StartCoroutine(TypeText(npcText, npcDialogues[currentDialogueIndex]));

        }
        else if (!isNPCTurn && currentDialogueIndex < playerDialogues.Length)
        {
            // 플레이어 대사 표시
            playerDialoguePanel.SetActive(true);
            npcDialoguePanel.SetActive(false);
            typingCoroutine = StartCoroutine(TypeText(playerText, playerDialogues[currentDialogueIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        isTyping = true;
        textComponent.text ="";

        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void ShowNextDialogue()
    {
        if ((isNPCTurn && currentDialogueIndex >= npcDialogues.Length) || !isNPCTurn && currentDialogueIndex >= playerDialogues.Length)
        {
            EndDialogue();
            return;
        }

        isNPCTurn = !isNPCTurn;

        if (isNPCTurn)
        {
            currentDialogueIndex++;
        }

        ShowCurrentDialogue();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        npcDialoguePanel.SetActive(false);
        playerDialoguePanel.SetActive(false);

        // 플레이어 이동 스크립 활성화
        if (player != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;
        }
    }
}

