using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Objects")]
    // 플레이어 오브젝트 (인스펙터 할당)
    public GameObject player;
    // 게임 UI (인스펙터 할당)
    public GameObject gameUI;
    // UI 이벤트 시스템 (인스펙터 할당)     
    public EventSystem gameEventSystem;

    [Header("Settings")]
    public string defaultNickname = "Guest";
    private string playerNickname;
    public TextMeshProUGUI playerNicknameText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);  // GameManager를 먼저 보존
            InitializePersistentObjects();       // 그 다음 플레이어와 UI 보존
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 게임 시작시 커서 설정(보이기)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 초기 닉네임 로드
        playerNickname = PlayerPrefs.GetString("PlayerNickname", defaultNickname);
        if (playerNicknameText != null)
        {
            playerNicknameText.text = playerNickname;
        }
    }

    void InitializePersistentObjects()
    {
        if (player != null)
        {
            DontDestroyOnLoad(player);
            player.SetActive(false); //초기에는 비활성화
        }
        if (gameUI != null)
        {
            DontDestroyOnLoad(gameUI);
        }
        if (gameEventSystem != null)
        {
            DontDestroyOnLoad(gameEventSystem);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LoginScene")
        {
            if (player != null)
            {
                player.SetActive(true);
            }
            if (gameUI != null)
            {
                gameUI.SetActive(true);
            }
        }
        else
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) DontDestroyOnLoad(player);
            }
            SetupNewScene();
        }
    }

    void SetupNewScene()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                DontDestroyOnLoad(player);
            }
            else
            {
                Debug.LogError("Player object not found in the scene!");
                return;
            }
        }

        PositionPlayer();
        ConfigureUI();
        CleanDuplicateEventSystems();
    }

    public void PositionPlayer()
    {
        if (player == null) return;

        player.SetActive(true);

        // SpawnPoint 찾고 플레이어 위치 동기화
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning("SpawnPoint not found. Placing player at (0,0,0)");
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;
        }
    }

    public void ConfigureUI()
    {
        if (gameUI == null) return;

        gameUI.SetActive(true);

        // Canvas 설정
        Canvas canvas = gameUI.GetComponent<Canvas>();
        if (canvas)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }

    // 중복 EventSystem 제거
    public void CleanDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EventSystem es in eventSystems)
        {
            if (es != gameEventSystem)
            {
                Destroy(es.gameObject);
            }
        }
    }

    // 씬 로드
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (player != null) player.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) DontDestroyOnLoad(player);
        }
        else
        {
            player.SetActive(true);
        }
    }

    // 닉네임 설정
    public void SetPlayerNickname(string nickname)
    {
        playerNickname = nickname;
        PlayerPrefs.SetString("PlayerNickname", nickname);
        if (playerNicknameText != null)
        {
            playerNicknameText.text = nickname;
        }
    }

    // 닉네임 값 가져오기
    public string GetPlayerNickname() => playerNickname;
}