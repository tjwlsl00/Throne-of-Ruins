using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{

    [Header("Dungeon Transition")]
    //로드할 다음 씬 이름
    public string nextSceneName;

    //플레이거가 트리거 영역 안에 있는지 추적하는 변수 
    private bool isPlayerInsideTrigger = false;

    void Start()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning($"{gameObject.name} 오브젝트에 붙은 SceneTransitionTrigger: 다음 씬 이름(nextSceneName)이 설정되지 않았습니다!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = true;
            Debug.Log("플레이어가 씬 전환 영역에 진입했습니다. 위쪽 화살표 키를 눌러 이동하세요.");

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

    void Update()
    {
        if (isPlayerInsideTrigger && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("위쪽 화살표 감지됨, 씬 로드 시도");
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        // Inspector에서 설정된 다음 씬 이름이 유효한지 확인
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // SceneManager를 직접 사용하여 씬 전환
            Debug.Log($"--- 씬 로드 요청: {nextSceneName} (SceneManager 직접 호출) ---");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 오브젝트에 붙은 SceneTransitionTrigger: 로드할 다음 씬 이름(nextSceneName)이 설정되지 않았습니다!", this);
        }
    }
}
