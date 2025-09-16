using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class purchaseUI : MonoBehaviour
{
    #region 내부 변수
    [SerializeField] private GameObject purchasePanel;
    [SerializeField] private GameObject failPanel;
    // 오디오
    public AudioSource audioSource;
    public AudioClip SucessSound;
    public AudioClip FailedSound;
    #endregion

    void Start()
    {
        purchasePanel.SetActive(false);
        failPanel.SetActive(false);
    }

    public void ShowpurchasePanel()
    {
        purchasePanel.SetActive(true);
        Debug.Log("구매 완료!");

        // 사운드
        audioSource.clip = SucessSound;
        audioSource.Play();
    }

    public void ShowFailPanel()
    {
        failPanel.SetActive(true);
        Debug.Log("구매 실패!");

        // 사운드
        audioSource.clip = FailedSound;
        audioSource.Play();
    }

    public void HidepurchasePanel()
    {
        purchasePanel.SetActive(false);
        Debug.Log("창 닫기");
    }

    public void HideFailPanel()
    {
        failPanel.SetActive(false);
        Debug.Log("창 닫기"); 
    }
}
