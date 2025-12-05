using UnityEngine;
using System.Collections.Generic;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ship (or object) that moves forward. Asteroids will spawn ahead of this transform.")]
    public Transform shipTransform;

    [Tooltip("Prefab(s) to use for asteroids. At least one required.")]
    public GameObject[] asteroidPrefabs;
   
    [Tooltip("Total number of asteroids kept alive (pooled).")]
    public int poolSize = 50;

    [Tooltip("Minimum Z offset ahead of the ship (positive).")]
    public float minZOffset = 30f;
    [Tooltip("Maximum Z offset ahead of the ship (positive).")]
    public float maxZOffset = 200f;

    public float spawnWidth = 40f;
    [Tooltip("Vertical range on Y axis: spawn between minY..maxY.")]
    public float minY = -5f;
    public float maxY = 12f;

    
    public float recycleBehindDistance = 20f;

    public Vector2 scaleRange = new Vector2(0.6f, 2.0f);
    public bool randomRotation = true;
    public int randomSeed = 0;
   
    public bool requirePositiveWorldZ = false;

    // internal pool
    List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        if (shipTransform == null)
        {   
            enabled = false;
            return;
        }
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0)
        {    
            enabled = false;
            return;
        }

        if (randomSeed != 0) Random.InitState(randomSeed);

        // Create pool
        pool = new List<GameObject>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            GameObject go = Instantiate(prefab, transform);
            go.name = $"Asteroid_{i}_{prefab.name}";
            pool.Add(go);
            PositionAhead(go, initial: true);
        }
    }

    void Update()
    {
        // Recycle asteroids that fell behind the ship
        float shipZ = shipTransform.position.z;
        for (int i = 0; i < pool.Count; i++)
        {
            GameObject a = pool[i];
            if (a == null) continue;
            if (a.transform.position.z < shipZ - recycleBehindDistance)
            {
                PositionAhead(a);
            }
        }
    }
 
    void PositionAhead(GameObject asteroid, bool initial = false)
    {
        float shipZ = shipTransform.position.z;

        float zOffset;
        if (initial)
        {
           
            zOffset = Random.Range(minZOffset, maxZOffset);
        }
        else
        {
           
            zOffset = Random.Range(minZOffset, maxZOffset);
        }

        float spawnZ = shipZ + Mathf.Abs(zOffset); 
   
        float spawnX = Random.Range(-spawnWidth, spawnWidth);
        float spawnY = Random.Range(minY, maxY);
     
        if (requirePositiveWorldZ && spawnZ < 0f) spawnZ = Mathf.Abs(spawnZ);

        asteroid.transform.position = new Vector3(spawnX, spawnY, spawnZ);

        // random rotation
        if (randomRotation)
        {
            asteroid.transform.rotation = Random.rotation;
        }

        // random scale
        float s = Random.Range(scaleRange.x, scaleRange.y);
        asteroid.transform.localScale = Vector3.one * s;

        var rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    public GameObject SpawnOneAhead()
    {
        var prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        GameObject go = Instantiate(prefab, transform);
        PositionAhead(go);
        pool.Add(go);
        return go;
    }

    public void ResetPool()
    {
        foreach (var g in pool)
            if (g != null) Destroy(g);
        pool.Clear();
        Start(); 
    }
}
