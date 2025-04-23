using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Spawner : MonoBehaviour
{
    private Collider spawnArea;

    public GameObject[] fruitPrefabs;
    public GameObject bombPrefab;
    
    // Power-up prefabs
    public GameObject[] powerUpPrefabs;
    
    [Range(0f, 1f)]
    public float bombChance = 0.05f;
    
    [Range(0f, 1f)]
    public float powerUpChance = 0.08f;

    public float minSpawnDelay = 0.25f;
    public float maxSpawnDelay = 1f;

    public float minAngle = -15f;
    public float maxAngle = 15f;

    public float minForce = 18f;
    public float maxForce = 22f;

    public float maxLifetime = 5f;

    private void Awake()
    {
        spawnArea = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        StartCoroutine(Spawn());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(2f);

        while (enabled)
        {
            GameObject prefab = null;
            
            // Determine what to spawn
            float random = Random.value;
            
            if (random < bombChance) {
                // Spawn bomb
                prefab = bombPrefab;
            } 
            else if (random < bombChance + powerUpChance && powerUpPrefabs != null && powerUpPrefabs.Length > 0) {
                // Spawn power-up if we have any power-up prefabs
                prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            }
            else {
                // Spawn normal fruit
                prefab = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
            }

            // Skip if we don't have a valid prefab (shouldn't happen but just in case)
            if (prefab == null) {
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
                continue;
            }

            Vector3 position = new Vector3
            {
                x = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                y = Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
                z = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
            };

            Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(minAngle, maxAngle));

            GameObject spawnedObject = Instantiate(prefab, position, rotation);
            Destroy(spawnedObject, maxLifetime);

            float force = Random.Range(minForce, maxForce);
            spawnedObject.GetComponent<Rigidbody>().AddForce(spawnedObject.transform.up * force, ForceMode.Impulse);

            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
        }
    }
}
