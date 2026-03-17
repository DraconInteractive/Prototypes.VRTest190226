using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //singleton jargon
    private static EnemyManager _instance;

    public static EnemyManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }


    //Dev Note: consider events for the following functions: may be required to ensure spawning is completed before loading player etc
    public GameObject LoadEnemy(EnemyBaseSO enemyBaseSO)
    {
        //creates new enemy
        GameObject enemy = enemyBaseSO.EnemyPrefab;
        //assigns attributes to components
        enemy.GetComponent<MeshFilter>().mesh= enemyBaseSO.EnemyMesh;
        enemy.GetComponent<MeshRenderer>().sharedMaterial = enemyBaseSO.EnemyMaterial;

        //fills enemy test presets from data
        enemy.gameObject.name = enemyBaseSO.EnemyName;
        TestEnemy enemyData = enemy.AddComponent<TestEnemy>();
        enemyData.MaxHealth = enemyBaseSO.MaxHP;
        //returns loaded enemy
        return enemy;
    }

    public GameObject SpawnEnemy(GameObject loadedEnemy, Transform spawnLocation)
    {
        //NOTE: left here for post instantiation functionality
        GameObject spawnedEnemy = Instantiate(loadedEnemy, spawnLocation);
        return spawnedEnemy;
    }
}
