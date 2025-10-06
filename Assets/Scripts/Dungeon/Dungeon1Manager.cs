using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon1Manager : MonoBehaviour
{
    public GameObject enemyPrefab; // 적 프리팹
    public List<Transform> spawnPoints;   // 스폰 위치
    public float respawnDelay = 5f; // 리스폰 지연 시간

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            SpawnEnemy(spawnPoints[i]); // 게임 시작 시 적 생성
        }
    }

    // 적 생성 함수
    void SpawnEnemy(Transform spawnPoint)
    {
        if (enemyPrefab != null && spawnPoint != null)
        {

            // 프리팹을 스폰 위치에 생성
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(newEnemy);
            Debug.Log($"적 생성됨: {newEnemy.name} at {spawnPoint.position}");

            // 생성된 적의 Enemy 스크립트를 가져옴
            Wizard enemyScript = newEnemy.GetComponent<Wizard>();
            if (enemyScript != null)
            {
                enemyScript.SetEnemyManager(this); // 적에게 EnemyManager 참조 전달
                enemyScript.SetMySpawnPoint(spawnPoint);
                enemyScript.InitializeEnemy();     // 적 초기화
            }
            else
            {
                Debug.LogWarning($"생성된 적 오브젝트에 Enemy 스크립트가 없습니다: {newEnemy.name}");
            }
        }
        else
        {
            Debug.LogError("적 프리팹 또는 스폰 위치가 설정되지 않았습니다!");
        }
    }

    // 적이 죽었을 때 호출될 함수
    public void EnemyDied(Wizard deadEnemy, Transform spawnPointWhereItDied)
    {
        Debug.Log($"{deadEnemy.gameObject.name} 죽음 신호 받음. {respawnDelay}초 후 리스폰 시작.");

        activeEnemies.Remove(deadEnemy.gameObject);

        // 리스폰 코루틴 시작
        StartCoroutine(RespawnCoroutine(spawnPointWhereItDied));
    }

    // 리스폰 지연 시간을 기다리는 코루틴
    IEnumerator RespawnCoroutine(Transform spawnPointToUse)
    {
        yield return new WaitForSeconds(respawnDelay);

        // 새 적 생성
        SpawnEnemy(spawnPointToUse);
    }
}
