using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TryBossBtn : MonoBehaviour
{
    [System.Serializable]
    public class PortalConnection
    {
        public string targetSceneName;  // 이동할 씬 이름
        public Vector3 linkedPortalPosition; // 연결된 포탈의 위치 (인스펙터에서 직접 지정)
    }

    public PortalConnection connection;
    //오디오 연결 
    public AudioSource audioSource;
    public AudioClip BtnClickSound;

    public void TryBoss()
    {
        // 오디오 재생
        audioSource.clip = BtnClickSound;
        audioSource.Play();
        StartCoroutine(TransitionScene());
    }

    IEnumerator TransitionScene()
    {
        // 오디오가 재생될 시간
        yield return new WaitForSeconds(1.0f);

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
