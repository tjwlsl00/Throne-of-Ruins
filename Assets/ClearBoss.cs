using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearBoss : MonoBehaviour
{
    [System.Serializable]
    public class PortalConnection
    {
        public string linkedPortalID;   // 연결된 포탈 ID (예: "Cave_Exit")
        public Vector3 linkedPortalPosition; // 연결된 포탈의 위치 (인스펙터에서 직접 지정)
    }
    
    #region 내부 변수
    public GameObject ClearBossPanel;
    // 카운팅 -> 최종 클리어 씬으로 이동
    private float CountingTime = 3f;
    private string finalScene = "finalScene";
    // 오디오
    public AudioSource audioSource;
    public AudioClip VictorySound;
    // 한 번만 실행하기 위한 플래그 변수
    private bool isClear = false;
    // 포탈 오브젝트 연결 
    public PortalConnection connection;
    #endregion

    void Start()
    {
        // audioSource가 null일 경우를 대비해 GetComponent를 호출합니다.
        // Inspector에서 이미 할당했다면 이 코드는 안전장치 역할을 합니다.
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        ClearBossPanel.SetActive(false);
    }

    void Update()
    {
        // 보스가 죽었고, 아직 클리어 처리를 하지 않았다면
        if (Boss.Instance.isDead && !isClear)
        {
            // 클리어 처리를 했다고 표시하여 다음 프레임부터는 실행되지 않도록 함
            isClear = true;
            BossSceneToFinalScene();
        }
    }

    #region 3초 카운팅 -> 최종 씬 이동
    public void BossSceneToFinalScene()
    {
        StartCoroutine(MoveToFinalScene());
    }

    IEnumerator MoveToFinalScene()
    {
        // 보스 애니메이션 시간 ㄱㄷ
        yield return new WaitForSeconds(3f);
        ClearBossPanel.SetActive(true);
        PlayClearSound();
        // 패널 활성화 이후 3초 뒤 씬 이동
        Debug.Log("3초 후 최종 씬으로 이동합니다.");
        yield return new WaitForSeconds(CountingTime);
        // 1. 연결된 포탈 위치 저장 
        PlayerPrefs.SetFloat("SpawnPosX", connection.linkedPortalPosition.x);
        PlayerPrefs.SetFloat("SpawnPosY", connection.linkedPortalPosition.y);
        PlayerPrefs.SetFloat("SpawnPosZ", connection.linkedPortalPosition.z);
        SceneManager.LoadScene(finalScene);
    }
    
     public void PlayClearSound()
    {
        audioSource.clip = VictorySound;
        audioSource.Play();
    }
    #endregion
}