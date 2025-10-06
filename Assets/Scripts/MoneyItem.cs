using System.Collections;
using UnityEngine;

public class MoneyItem : MonoBehaviour
{
    public int value = 10;
    private bool playerCanPickUp = false;

    // 오디오 
    public AudioClip PickupEffect;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 플레이어가 아이템을 주울 수 있는 상태이고, 'Z' 키를 눌렀을 때
        if (playerCanPickUp && Input.GetKeyDown(KeyCode.Z))
        {
            PickUP();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCanPickUp = true; // 플레이어가 주울 수 있는 상태로 변경
            Debug.Log("플레이어가 돈 아이템 범위에 들어왔습니다. Z키를 눌러 주울 수 있습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCanPickUp = false;
            Debug.Log("플레이어가 돈 아이템 범위에서 나갔습니다.");
        }
    }

    private void PickUP()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.Addmoney(value);
            Debug.Log(value + "돈 획득! 현재 $: " + PlayerState.Instance.money);

            // 오디오 재생(돈 줍기)
            audioSource.clip = PickupEffect;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("씬에 playerState 스크립트가 없습니다.");
        }

        // 오디오 재생이 끝난 후 파괴
        StartCoroutine(DestroyAfterSound());
    }

    private IEnumerator DestroyAfterSound()
    {
        yield return new WaitForSeconds(0.5f);
        // 아이템을 주웠으므로 파괴합니다.
        Destroy(gameObject);
    }
}