using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [System.Serializable]
    public class PortalConnection
    {
        public string targetSceneName;  // 이동할 씬 이름
        public string linkedPortalID;   // 연결된 포탈 ID (예: "Cave_Exit")
        public Vector3 linkedPortalPosition; // 연결된 포탈의 위치 (인스펙터에서 직접 지정)
    }

    public PortalConnection connection;

    //오디오 연결 
    public AudioClip PortalEffect;
    public AudioSource audioSource;

    //플레이거가 트리거 영역 안에 있는지 추적하는 변수
    private bool isPlayerInsideTrigger = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = true;
            Debug.Log($"포탈 접근: {connection.linkedPortalID}로 이동 가능 (↑키)");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = false;
            Debug.Log("플레이어가 씬 전환 영역에서 벗어났습니다.");
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isPlayerInsideTrigger && Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 오디오 재생
            audioSource.clip = PortalEffect;
            audioSource.Play();

            // 포탈 타는 소리를 위해 코루틴 사용 
            StartCoroutine(TransitionScene());
        }
    }

    private IEnumerator TransitionScene()
    {
        // 오디오가 재생될 시간
        yield return new WaitForSeconds(1.0f);

        // 1. 연결된 포탈 위치 저장 
        PlayerPrefs.SetFloat("SpawnPosX", connection.linkedPortalPosition.x);
        PlayerPrefs.SetFloat("SpawnPosY", connection.linkedPortalPosition.y);
        PlayerPrefs.SetFloat("SpawnPosZ", connection.linkedPortalPosition.z);
        // 2. 씬 전환
        if (!string.IsNullOrEmpty(connection.targetSceneName))
        {
            SceneManager.LoadScene(connection.targetSceneName);
        }
        else
        {
            Debug.Log("타겟 씬 이름이 지정되지 않았습니다.");
        }
    }
}
