using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    #region 내부 변수 
    public static LoginManager Instance;
    public string sceneAfterLogin = "Viliage";
    public TMP_InputField nicknameInputField;
    public GameObject welcomePanel;
    public TMP_Text welcomeText;
    public Button startButton;
    public Button explainButton;
    public GameObject explainPanel;
    public GameObject[] panels;
    private int currentIndex = 0;
    public GameObject inputFieldGameObject;
    private string playerNickname;
    // 오디오 
    public AudioSource audioSource;
    public AudioClip BtnClickSound;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        explainPanel.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoginScene")
        {
            InitializeLoginScene();
        }
    }

    void InitializeLoginScene()
    {
        welcomePanel?.SetActive(false);
        startButton?.gameObject.SetActive(false);
        explainButton?.gameObject.SetActive(false);
        inputFieldGameObject?.SetActive(true);

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
            nicknameInputField?.Select();
        }

    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoginScene")
        {
            if (nicknameInputField == null) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ProcessLogin();
            }
        }
    }

    #region 닉네임 입력 후 -> 환영합니다 메세지
    void ProcessLogin()
    {
        string nickname = nicknameInputField?.text;

        if (!string.IsNullOrEmpty(nickname))
        {
            playerNickname = nickname;
            PlayerPrefs.SetString("PlayerNickname", nickname);
            PlayerPrefs.Save();

            if (welcomePanel != null)
            {
                welcomePanel.SetActive(true);
                if (welcomeText != null)
                {
                    welcomeText.text = "ようこそ、" + nickname + "様";
                }
            }

            inputFieldGameObject?.SetActive(false);
            startButton?.gameObject.SetActive(true);
            explainButton?.gameObject.SetActive(true);
        }
        else
        {
            nicknameInputField?.ActivateInputField();
        }
    }
    #endregion

    #region 버튼 클릭 -> 닉네임 가져오기 -> 닉네임 전달
    public void OnStartButtonClicked()
    {
        // GameManager에 닉네임 전달
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerNickname(playerNickname);
        }

        // 로그인 UI 정리
        nicknameInputField = null;
        welcomePanel = null;
        welcomeText = null;
        startButton = null;
        inputFieldGameObject = null;
        SceneManager.LoadScene(sceneAfterLogin);

        // 사운드 
        audioSource.clip = BtnClickSound;
        audioSource.Play();
    }

    public string GetPlayerNickname()
    {
        return playerNickname;
    }
    #endregion

    #region 게임 설명 버튼 클릭 -> 패널 열기
    public void GameExplainButtonCliked()
    {
        if (explainPanel != null)
        {
            explainPanel.SetActive(true);
            ShowPanelAtIndex(currentIndex);
        }
        else
        {
            Debug.LogError("오류: targetPanel이 연결되지 않았습니다!");
        }

        audioSource.clip = BtnClickSound;
        audioSource.Play();
    }

    public void ClosePanel()
    {
        if (explainPanel != null)
        {
            explainPanel.SetActive(false);
            currentIndex = 0;
        }
        else
        {
            Debug.LogError("오류: targetPanel이 연결되지 않았습니다!");
        }

        audioSource.clip = BtnClickSound;
        audioSource.Play();
    }
    #endregion

    #region 패널 전환
    public void ShowPanelAtIndex(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }

    public void ShowNextPanel()
    {
        currentIndex++;

        if (currentIndex >= panels.Length)
        {
            currentIndex = 0;
        }

        ShowPanelAtIndex(currentIndex);
        
        // 오디오
        audioSource.clip = BtnClickSound;
        audioSource.Play();
    }
    #endregion
}