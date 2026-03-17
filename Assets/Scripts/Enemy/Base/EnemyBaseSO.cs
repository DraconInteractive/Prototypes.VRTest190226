using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Enemy", order = 1)]
public class EnemyBaseSO : ScriptableObject
{
    //enemy creation data
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private string enemyName;
    [SerializeField] private float maxHP;
    [SerializeField] private string description;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Mesh enemyMesh;
    [SerializeField] public bool isAlive;

    public GameObject EnemyPrefab => enemyPrefab;
    public string EnemyName => enemyName;
    public float MaxHP => maxHP;
    public string Description => description;
    public Material EnemyMaterial => enemyMaterial;

    public Mesh EnemyMesh => enemyMesh;
    public bool IsAlive => isAlive;
}
